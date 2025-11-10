using AllAccessApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFileAsync(FileUploadDto fileDto, int userId);
        Task<FileDownloadResponse> DownloadFileAsync(int fileId, int userId);
        Task<FileDownloadResponse> ViewFileAsync(int fileId, int userId);
        Task<FileListResponse> GetUserFilesAsync(int userId);
        Task<FileUploadResponse> DeleteFileAsync(int fileId, int userId);
    }
}
