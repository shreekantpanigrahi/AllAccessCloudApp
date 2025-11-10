using AllAccessApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterModel model);
        Task<AuthResponse> LoginAsync(LoginModel model);
        Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordModel model);  
        Task<AuthResponse> VerifyOtpAsync(VerifyOtpModel model);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordModel model);
    }
}
