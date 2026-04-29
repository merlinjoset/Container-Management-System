using ContainerManagement.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ContainerManagement.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Email:Host"]) &&
        !string.IsNullOrWhiteSpace(_config["Email:Username"]) &&
        !string.IsNullOrWhiteSpace(_config["Email:Password"]);

    public async Task<bool> SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toAddress))
        {
            _logger.LogWarning("SmtpEmailSender: No recipient address provided.");
            return false;
        }

        if (!IsConfigured)
        {
            _logger.LogWarning("SmtpEmailSender: SMTP not configured. Subject={Subject} To={To}", subject, toAddress);
            return false;
        }

        try
        {
            var host = _config["Email:Host"]!;
            var port = int.TryParse(_config["Email:Port"], out var p) ? p : 587;
            var enableSsl = !string.Equals(_config["Email:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);
            var user = _config["Email:Username"]!;
            var pass = _config["Email:Password"]!;
            var fromAddr = _config["Email:FromAddress"] ?? user;
            var fromName = _config["Email:FromName"] ?? "Container Management";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            using var msg = new MailMessage
            {
                From = new MailAddress(fromAddr, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(toAddress);

            await client.SendMailAsync(msg, ct);
            _logger.LogInformation("SmtpEmailSender: Sent email '{Subject}' to {To}", subject, toAddress);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SmtpEmailSender: Failed sending email to {To}", toAddress);
            return false;
        }
    }
}
