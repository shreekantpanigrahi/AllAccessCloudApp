using AllAccessApp.Application.DTOs;
using AllAccessApp.Application.Services;
using AllAccessApp.Domain.Entities;
using AllAccessApp.Infrastructure.Context;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private const long CompressThreshold = 52_428_800; // 50 MB

        public FileService(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<FileUploadResponse> UploadFileAsync(FileUploadDto fileDto, int userId)
        {
            //1. Validate file
            if (fileDto == null || fileDto.FileStream == null || fileDto.Length == 0)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = "No file uploaded. 🤧🤧🤧",
                };
            }

            //2. Max File Size : 100Mb 
            const long MaxFileSize = 104_857_600;
            if (fileDto.Length > MaxFileSize)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = "File too large. Maximum size is 100 MB 😭😭😭."
                };
            }

            //3. Get User
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            //5. Generate Unique File name 
            var safeOriginalName = Path.GetFileName(fileDto.FileName ?? "unknown");
            var fileExtension = Path.GetExtension(safeOriginalName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(safeOriginalName);
            var fileName = $"{Guid.NewGuid()}_{fileNameWithoutExt}{fileExtension}";

            //6. R2 Config 
            var s3client = CreateS3Client();
            if(s3client == null)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = $"R2 configuration is missing or incomplete."
                };
            }

            var bucketName = _config["R2:BucketName"];
            var originalFileSize= fileDto.Length;
            var shouldCompress= fileDto.Length > CompressThreshold;
            var isCompressed = false;
            var compressionRatio = 1.0f;

            var memoryStream = new MemoryStream();
            try
            {
                await fileDto.FileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Compress if file is larger than 50MB
                MemoryStream finalStream = memoryStream;
                if (shouldCompress)
                {
                    var compressedStream = await CompressStreamAsync(memoryStream);
                    if(compressedStream != null && compressedStream.Length < memoryStream.Length)
                    {
                        finalStream = compressedStream;
                        isCompressed = true;
                        compressionRatio = (float)finalStream.Length / (float)memoryStream.Length;
                        fileName = Path.ChangeExtension(fileName, ".gz"); // Add .gz extension
                        memoryStream.Dispose(); // Dispose original stream
                    }
                    else
                    {
                        compressedStream?.Dispose(); // Dispose failed compression stream
                    }
                }

                //6. Check Quota with final file size
                if (user.UsedStorage + finalStream.Length > user.StorageQuota)
                {
                    return new FileUploadResponse
                    {
                        Success = false,
                        Message = $"Not enough space. You have used {FormatSize(user.UsedStorage)} of {FormatSize(user.StorageQuota)}."
                    };
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = finalStream,
                    ContentType = isCompressed ? "application/gzip" : (fileDto.ContentType ?? "application/octet-stream"),
                    DisablePayloadSigning = true,
                    AutoCloseStream = false,
                    DisableDefaultChecksumValidation = true
                };

                putRequest.Headers.ContentLength = finalStream.Length;

                var response = await s3client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    return new FileUploadResponse
                    {
                        Success = false,
                        Message = $"Failed to upload file to storage. Status: {response.HttpStatusCode}"
                    };
                }

                //7. Save Metadata 
                var fileItem = new FileItem
                {
                    FileName = fileName,
                    OriginalName = safeOriginalName,
                    FileType = fileDto.ContentType ?? "application/octet-stream",
                    FileSize = finalStream.Length,
                    OriginalFileSize = originalFileSize,
                    IsCompressed = isCompressed,
                    FilePath = fileName,
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };

                _context.Files.Add(fileItem);
                await _context.SaveChangesAsync();

                //8. Update User storage 
                user.UsedStorage += finalStream.Length;
                await _context.SaveChangesAsync();

                var endpoint = _config["R2:Endpoint"];
                //9. Return Response 
                return new FileUploadResponse
                {
                    Success = true,
                    Message = isCompressed
                        ? $"File uploaded and compressed successfully! Saved {FormatSize(originalFileSize - finalStream.Length)} space 😊"
                        : "File uploaded successfully 😊😊😊",
                    FileName = fileItem.FileName,
                    FilePath = $"{endpoint?.TrimEnd('/')}/{bucketName}/{fileName}",
                    FileSize = fileItem.FileSize,
                    OriginalFileSize = originalFileSize,
                    FileType = fileItem.FileType,
                    FileId = fileItem.Id,
                    IsCompressed = isCompressed,
                    CompressionRatio = compressionRatio
                };
            }
            catch (AmazonS3Exception s3ex)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = $"R2 error: {s3ex.Message} (Code: {s3ex.ErrorCode})"
                };
            }
            catch (Exception ex)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = $"Upload error: {ex.Message}"
                };
            }
            finally
            {
                memoryStream?.Dispose();
            }
        }

        public async Task<FileDownloadResponse> DownloadFileAsync(int fileId, int userId)
        {
            try
            {
                var fileItem = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);

                if (fileItem == null)
                {
                    return new FileDownloadResponse
                    {
                        Success=false,
                        Message="File Not found or access denied!!!😒😒😒"
                    };
                }

                var s3client = CreateS3Client();
                var bucketName = _config["R2:BucketName"];

                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileItem.FilePath
                };

                var response = await s3client.GetObjectAsync(getRequest);
                var fileStream = response.ResponseStream;

                Console.WriteLine($"Final stream length: {fileStream.Length}");
                if (fileItem.IsCompressed)
                {
                    //var decompressedStream = await DecompressStreamAsync(fileStream);
                    //fileStream.Dispose();
                    //fileStream = decompressedStream;
                    fileStream = new GZipStream(fileStream, CompressionMode.Decompress, leaveOpen: false);
                }

                return new FileDownloadResponse
                {
                    Success = true,
                    Message = "File downloaded successfully",
                    FileStream = fileStream,
                    FileName = fileItem.OriginalName,
                    ContentType = fileItem.FileType,
                    //FileSize = fileItem.IsCompressed ? fileItem.OriginalFileSize : fileItem.FileSize
                    FileSize= fileItem.OriginalFileSize
                };

            }catch(Exception ex)
            {
                return new FileDownloadResponse
                {
                    Success = false,
                    Message = $"Download Error:{ex.Message}"
                };
            }
        }

        public async Task<FileDownloadResponse> ViewFileAsync(int fileId, int userId)
        {
            return await DownloadFileAsync(fileId, userId);
        }

        public async Task<FileListResponse> GetUserFilesAsync(int userId)
        {
            try
            {
                // First, get the data from database WITHOUT calling FormatSize
                var filesData = await _context.Files.Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedOn)
                    .Select(f => new 
                    {
                        f.Id,
                        f.FileName,
                        f.OriginalName,
                        f.FileType,
                        f.FileSize,
                        f.OriginalFileSize,
                        f.IsCompressed,
                        f.CreatedOn,
                        f.ModifiedOn

                    }).ToListAsync();

                // Then, format the sizes in memory (not on database)
                var files = filesData.Select(f => new FileItemDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    OriginalName = f.OriginalName,
                    FileType = f.FileType,
                    FileSize = f.FileSize,
                    OriginalFileSize = f.OriginalFileSize,
                    IsCompressed = f.IsCompressed,
                    CreatedOn = f.CreatedOn,
                    ModifiedOn = f.ModifiedOn,
                    FormattedSize = FormatSize(f.FileSize), // ✅ Now this runs in memory
                    FormattedOriginalSize = FormatSize(f.OriginalFileSize) // ✅ This too
                }).ToList();

                return new FileListResponse
                {
                    Success = true,
                    Message = $"Found {files.Count} files",
                    Files = files
                };
            }catch(Exception ex)
            {
                return new FileListResponse
                {
                    Success = false,
                    Message = $"Error retrieving files : {ex.Message}"
                };
            }
        }

        public async Task<FileUploadResponse> DeleteFileAsync(int fileId, int userId)
        {
            try
            {

                var fileItem = await _context.Files.FirstOrDefaultAsync(f => f.Id == fileId && f.UserId == userId);
                if (fileItem == null)
                {
                    return new FileUploadResponse
                    {
                        Success = false,
                        Message = "File not found or access denied"
                    };
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new FileUploadResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var s3client = CreateS3Client();
                var bucketName = _config["R2:BucketName"];

                // Delete from R2
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileItem.FilePath
                };

                await s3client.DeleteObjectAsync(deleteRequest);

                // Update user storage
                user.UsedStorage -= fileItem.FileSize;

                // Remove from database
                _context.Files.Remove(fileItem);
                await _context.SaveChangesAsync();

                return new FileUploadResponse
                {
                    Success = true,
                    Message = "File deleted successfully 🗑️"
                };
            }
            catch (Exception ex) 
            {
                return new FileUploadResponse
                {
                    Success = false,
                    Message = $"Delete error: {ex.Message}"
                };
            }
        }

        private AmazonS3Client? CreateS3Client()
        {
            var accessKey = _config["R2:AccessKey"];
            var secretKey = _config["R2:SecretKey"];
            var endpoint = _config["R2:Endpoint"];

            if(string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(endpoint))
            {
                return null;
            }
            var s3Config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "auto",
                UseHttp = false
            };
            return new AmazonS3Client(accessKey, secretKey, s3Config);
        }

        private async Task<MemoryStream> CompressStreamAsync(Stream inputStream)
        {
            try
            {
                var compressedStream = new MemoryStream();
                inputStream.Position = 0;

                using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
                {
                    await inputStream.CopyToAsync(gzipStream);
                }
                
                // ensure all data is flushed
                compressedStream.Position = 0;
                return compressedStream;
            }
            catch
            {
                return null;
            }
        }

        private async Task<MemoryStream> DecompressStreamAsync(Stream compressedStream)
        {
            try
            {
                var seekableStream = new MemoryStream();
                await compressedStream.CopyToAsync(seekableStream);
                seekableStream.Position = 0;

                Console.WriteLine($"Decompressing stream of size: {compressedStream.Length}");

                var decompressedStream = new MemoryStream();
                using (var gzipStream = new GZipStream(seekableStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    await gzipStream.CopyToAsync(decompressedStream);
                }

                decompressedStream.Position = 0; // Reset position for reading
                Console.WriteLine($"Decompression successful: {decompressedStream.Length} bytes");
                return decompressedStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decompression error: {ex.Message}");
                throw new InvalidOperationException($"Failed to decompress file: {ex.Message}", ex);
            }
        }


        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}