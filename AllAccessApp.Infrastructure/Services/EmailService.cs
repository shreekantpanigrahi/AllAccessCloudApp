using AllAccessApp.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<bool> SendContactEmailAsync(string name, string email, string message)
        {
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(
                     emailSettings["DisplayName"],
                    emailSettings["FromEmail"]));

                mimeMessage.To.Add(new MailboxAddress("Admin", emailSettings["FromEmail"])); 

                mimeMessage.Subject = $"Contact Form: {name} ({email})";

                var bodyBuilder= new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <h3>New Contact Message</h3>
                    <p><strong>Name:</strong> {name}</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Message:</strong></p>
                    <blockquote>{message}</blockquote>
                    <p><em>Sent from AllAccessApp</em></p>";

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings["SmtpServer"],
                                          emailSettings.GetValue<int>("Port"),
                                           SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);

                return true;

            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string email, string name, string otp)
        {
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(emailSettings["DisplayName"], emailSettings["FromEmail"]));
                mimeMessage.To.Add(new MailboxAddress(name, email));
                mimeMessage.Subject = "AllAccessApp Password Reset OTP";

                var bodyBuilder= new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <h3>Password Reset Request</h3>
                    <p>Hi {name},</p>
                    <p>You requested to reset your password. Use the following OTP:</p>
                    <h2 style='letter-spacing: 5px;'>{otp}</h2>
                    <p>This code expires in 10 minutes.</p>
                    <p>If you didn't request this, ignore this email.</p>
                    <br>
                    <em>— AllAccessApp Team</em>";

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(emailSettings["SmtpServer"],
                                           emailSettings.GetValue<int>("Port"),
                                           SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
