using AllAccessApp.Application.DTOs;
using AllAccessApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync (int  userId, UpdateProfileModel profile);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordModel model);
        Task<string?> UploadProfilePictureAsync(int userId, IFormFile file);
    }
}
