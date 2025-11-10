using AllAccessApp.Application.Services;
using AllAccessApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AllAccessApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {

        private readonly IEmailService _emailService;

        public ContactController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ContactFormModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _emailService.SendContactEmailAsync(model.Name, model.Email, model.Message);

            if (!success)
                return StatusCode(500, new { message = "Failed to send email" });

            return Ok(new { message = "Thank you! Your message has been sent." });
        }
    }
}
