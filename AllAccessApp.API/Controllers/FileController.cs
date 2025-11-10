using AllAccessApp.Application.DTOs;
using AllAccessApp.Application.Services;
using Amazon;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllAccessApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<FileUploadResponse>> UploadFile([FromForm] FileDTO fileDto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("User not Authenticated");
            }

            if (fileDto.File == null || fileDto.File.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadDto = new FileUploadDto
            {
                FileName = fileDto.File.FileName,
                ContentType = fileDto.File.ContentType,
                Length = fileDto.File.Length,
                FileStream = fileDto.File.OpenReadStream()
            };

            var result= await _fileService.UploadFileAsync(uploadDto, userId);
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpGet("my-files")]
        public async Task<ActionResult<FileListResponse>> GetUserFiles()
        {
            var userId= GetCurrentUserId();
            if(userId == 0)
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _fileService.GetUserFilesAsync(userId);
            return Ok(result);
        }

        [HttpGet("fileId/download")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var userId= GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _fileService.DownloadFileAsync(fileId, userId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            //if (!result.FileStream!.CanSeek)
            //{
            //    var memoryStream = new MemoryStream();
            //    await result.FileStream.CopyToAsync(memoryStream);
            //    result.FileStream.Dispose();
            //    memoryStream.Position = 0;
            //    result.FileStream = memoryStream;
            //}

            //return File(result.FileStream!, result.ContentType!, result.FileName);

            Response.Headers.Add("Content-Disposition", $"attachment; fileName=\"{result.FileName}\"");
            if (result.FileSize > 0)
            {
                Response.Headers.Add("Content-Length", result.FileSize.ToString());
            }

            // 🔑 Stream file directly to client (no buffering in memory)
            return File(result.FileStream!, result.ContentType ?? "application/octet-stream");
        }

        [HttpGet("{fileId}/view")]
        public async Task<IActionResult> ViewFile(int fileId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _fileService.ViewFileAsync(fileId, userId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            
            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{result.FileName}\"");
            return File(result.FileStream!, result.ContentType ?? "application/octet-stream");
        }

        [HttpDelete("{fileId}/delete")]
        public async Task<ActionResult<FileUploadResponse>> DeleteFile(int fileId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _fileService.DeleteFileAsync(fileId, userId);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpGet("{fileId}/info")]
        public async Task<ActionResult<FileItemDto>> GetFileInfo(int fileId)
        {
            var userId= GetCurrentUserId();
            if(userId == 0)
            {
                return Unauthorized("User not authenticated");
            }

            var filesResult= await _fileService.GetUserFilesAsync(userId);
            if(!filesResult.Success || filesResult.Files == null)
            {
                return BadRequest("Error retrieving files");
            }

            var fileInfo = filesResult.Files.FirstOrDefault(f => f.Id == fileId);
            if (fileInfo == null) 
            {
                return NotFound("File not found");
            }
            return Ok(fileInfo);
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
