using HotelReservationSystemAPI.Application.Interface;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Infrastructure.Implementation;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending confirmation email to {Recipient} with subject {Subject}", to, subject);

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("No Reply", _config["Email:From"] ?? "noreply@hotelRevservationSystem.com"));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls,
                CancellationToken.None);  

            await client.AuthenticateAsync(
                _config["Email:Username"] ?? "",
                _config["Email:Password"] ?? "");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Confirmation email sent successfully to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Recipient}", to);
            // Don't throw—log and continue (registration succeeds, email fails gracefully)
            // Optional: Retry logic or queue for later
        }
    }
}

