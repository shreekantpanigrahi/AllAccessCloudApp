using AllAccessApp.Application.DTOs;
using AllAccessApp.Application.Services;
using AllAccessApp.Domain.Entities;
using AllAccessApp.Infrastructure.Context;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;



namespace AllAccessApp.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(DataContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService= emailService;

        }
        public async Task<AuthResponse> RegisterAsync(RegisterModel model)
        {
            //1. Validate 
            if(await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exist"
                };
            }

            //2. Hash Password 
            var passwordhash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            //3. Create User
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = passwordhash,
                UsedStorage=0,
                StorageQuota = 524_288_000,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //4. Return Token 
            var token = GenerateJwtToken(user);
            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }
        public async Task<AuthResponse> LoginAsync(LoginModel model)
        {
            // 1. Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid Email"
                };
            }

            //2. Verify Password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message= "Invalid Password"
                };
            }

            //3. Generate Token 
            var token = GenerateJwtToken(user);
            return new AuthResponse
            {
                Success = true,
                Message = "Login Successful",
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiryInMinutes")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateOtp()
        {
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000;
            return (code + 100000).ToString("D6"); 
        }

        public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.Email == model.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "If this email exists, a password reset link has been sent."
                };
            }

            // Generate 6-digit OTP
            var otp = GenerateOtp();

            //Hash and store Otp
            user.ResetPasswordOtp = BCrypt.Net.BCrypt.HashPassword(otp);
            user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            //Send Email
            try
            {
                await _emailService.SendOtpEmailAsync(user.Email, user.Name, otp);
            }
            catch
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Failed to send email. Please try again."
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "If this email exists, a password reset OTP has been sent."
            };
        }

        public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if(user==null || string.IsNullOrEmpty(user.ResetPasswordOtp))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid Request"
                };
            }

            if(user.ResetPasswordOtpExpiry<DateTime.UtcNow || !user.ResetPasswordOtpExpiry.HasValue)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "OTP has expired. Please request a new one."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Otp, user.ResetPasswordOtp))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid OTP."
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "OTP verified successfully.",
                Email = user.Email
            };
        }

        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || string.IsNullOrEmpty(user.ResetPasswordOtp))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid reset request."
                };
            }

            if (user.ResetPasswordOtpExpiry < DateTime.UtcNow || !user.ResetPasswordOtpExpiry.HasValue)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "OTP has expired. Please start over."
                };
            }  
            
            if(model.NewPassword!= model.ConfirmPassword)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Passwords do not match."
                };
            }

            //Reset Password 
            user.PasswordHash= BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            //Clear Otp
            user.ResetPasswordOtp = null;
            user.ResetPasswordOtpExpiry = null;

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Password reset successfully. You can now log in."
            };
        }

    }
}
