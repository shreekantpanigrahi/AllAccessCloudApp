using AllAccessApp.Application.Services;
using AllAccessApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllAccessApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        { 
            var userId = GetCurrentUserId();
            var profile= await _profileService.GetUserProfileAsync(userId);
            return Ok(profile);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileModel model)
        {
            var userId = GetCurrentUserId();
            var success= await _profileService.UpdateUserProfileAsync(userId, model);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update profile" });
            }
            return Ok(new {message= "Profile updated successfully" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = GetCurrentUserId();
            var success = await _profileService.ChangePasswordAsync(userId,model);
            if (!success)
            {
                return BadRequest(new { message = "Current password is incorrect or update failed" });
            }
            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            var userId= GetCurrentUserId();
            try
            {
                var url = await _profileService.UploadProfilePictureAsync(userId, file);
                if(url == null)
                {
                    return BadRequest(new { message = "Upload failed" });
                }
                return Ok(new { profilePictureUrl = url });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
