using HotelReservationSystemAPI.Application.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Example using Gmail SMTP
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("noreply@hotelapi.com", "app-password"),
                    EnableSsl = true
                };

                var message = new MailMessage("noreply@hotelapi.com", to, subject, body)
                {
                    IsBodyHtml = true
                };

                await smtp.SendMailAsync(message);

                _logger.LogInformation("Email sent to {Recipient}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            }
        }

      
    }
}
