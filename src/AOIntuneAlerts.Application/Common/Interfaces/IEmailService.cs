namespace AOIntuneAlerts.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        IEnumerable<string> toAddresses,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
