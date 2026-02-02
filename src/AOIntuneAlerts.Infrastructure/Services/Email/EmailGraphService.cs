using AOIntuneAlerts.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace AOIntuneAlerts.Infrastructure.Services.Email;

public class EmailOptions
{
    public const string SectionName = "Email";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Intune Compliance Portal";
}

public class EmailGraphService : IEmailService
{
    private readonly GraphServiceClient _graphClient;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailGraphService> _logger;

    public EmailGraphService(
        GraphServiceClient graphClient,
        IOptions<EmailOptions> options,
        ILogger<EmailGraphService> logger)
    {
        _graphClient = graphClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        IEnumerable<string> toAddresses,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.SenderEmail))
        {
            _logger.LogWarning("Sender email not configured. Email not sent.");
            return;
        }

        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = htmlBody
            },
            ToRecipients = toAddresses.Select(email => new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = email
                }
            }).ToList()
        };

        var requestBody = new SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = true
        };

        try
        {
            await _graphClient.Users[_options.SenderEmail]
                .SendMail
                .PostAsync(requestBody, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Email sent successfully. Subject: {Subject}, Recipients: {RecipientCount}",
                subject, toAddresses.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email. Subject: {Subject}", subject);
            throw;
        }
    }
}
