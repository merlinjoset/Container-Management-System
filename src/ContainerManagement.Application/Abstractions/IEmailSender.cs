namespace ContainerManagement.Application.Abstractions;

public interface IEmailSender
{
    /// <summary>
    /// Send an email. Returns true on success, false if not configured / failed.
    /// </summary>
    Task<bool> SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>
    /// Whether SMTP credentials are configured. If false, SendAsync logs a warning and returns false.
    /// </summary>
    bool IsConfigured { get; }
}
