using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.Services
{
    public interface IEmailService
    {
        Task<bool> SendContactEmailAsync(string name, string email, string message);
        Task<bool> SendOtpEmailAsync(string email, string name, string otp);
    }
}
