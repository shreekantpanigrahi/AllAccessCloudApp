using AllAccessApp.Application.DTOs;
using AllAccessApp.Application.Services;
using AllAccessApp.Domain.Entities;
using AllAccessApp.Infrastructure.Context;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public ProfileService(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                UsedStorage = user.UsedStorage,
                StorageQuota = user.StorageQuota
            };
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId))
                return false;

            user.Name = model.Name;
            user.Email = model.Email;
            user.ModifiedOn = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> UploadProfilePictureAsync(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            const long MaxFileSize = 5_242_880; // 5 MB
            if (file.Length > MaxFileSize)
                throw new Exception("File too large. Maximum size is 5 MB.");

            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Only image files (JPEG, PNG) are allowed.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            // Generate a unique File name 
            var fileName= $"profile-{userId}-{Guid.NewGuid()}.jpg";

            //Upload to R2
            var accessKey = _config["R2:AccessKey"];
            var secretKey = _config["R2:SecretKey"];
            var bucketName = _config["R2:BucketName"];
            var endpoint = _config["R2:Endpoint"];

            using var s3Client= new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true
            });

            // 🔑 CRITICAL FIX: Read stream into memory and set ContentLength
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = file.OpenReadStream(),
                ContentType = "image/jpeg",
                DisablePayloadSigning = true,
                AutoCloseStream = false,
                DisableDefaultChecksumValidation = true
            };

            // 👇 Explicitly set Content-Length
            putRequest.Headers.ContentLength = memoryStream.Length;

            try
            {
                var response = await s3Client.PutObjectAsync(putRequest);
                if(response.HttpStatusCode != HttpStatusCode.OK)
                    return null;
            }
            catch(AmazonS3Exception s3Ex)
            {
                throw new Exception($"R2 Upload Failed: {s3Ex.Message}");
            }

            //Delete old Pictures
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var oldKey = user.ProfilePictureUrl.Split('/').Last();
                try {
                    await s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = oldKey
                    });
                } 
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to delete old profile picture: {ex.Message}");
                }
                
            }

            //Update user
            user.ProfilePictureUrl = $"{endpoint}/{fileName}";
            await _context.SaveChangesAsync();

            return user.ProfilePictureUrl;
        }
    }
}
