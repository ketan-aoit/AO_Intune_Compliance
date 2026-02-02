using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Aggregates.Alert;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.Application.Alerts.Commands;

public record SendAlertCommand : IRequest<Result>
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public AlertSeverity Severity { get; init; }
    public Guid? DeviceId { get; init; }
    public string? AlertType { get; init; }
}

public class SendAlertCommandHandler : IRequestHandler<SendAlertCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendAlertCommandHandler> _logger;

    public SendAlertCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<SendAlertCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendAlertCommand request, CancellationToken cancellationToken)
    {
        // Check cooldown if this is a device-specific alert
        if (request.DeviceId.HasValue && !string.IsNullOrEmpty(request.AlertType))
        {
            var cooldown = await _context.AlertCooldowns
                .FirstOrDefaultAsync(c =>
                    c.DeviceId == request.DeviceId.Value &&
                    c.AlertType == request.AlertType,
                    cancellationToken);

            if (cooldown?.IsInCooldown() == true)
            {
                _logger.LogInformation(
                    "Alert skipped due to cooldown. DeviceId: {DeviceId}, AlertType: {AlertType}",
                    request.DeviceId, request.AlertType);
                return Result.Success();
            }
        }

        // Get eligible recipients
        var recipients = await _context.AlertRecipients
            .Where(r => r.IsEnabled && r.MinimumSeverity <= request.Severity)
            .ToListAsync(cancellationToken);

        if (!recipients.Any())
        {
            _logger.LogWarning("No eligible recipients for alert: {Subject}", request.Subject);
            return Result.Success();
        }

        // Create alert record
        var alert = Alert.Create(
            request.Subject,
            request.Body,
            request.Severity,
            request.DeviceId);

        foreach (var recipient in recipients)
        {
            alert.AddRecipient(recipient);
        }

        _context.Alerts.Add(alert);

        try
        {
            // Send email
            var toAddresses = recipients.Select(r => r.Email.Value).ToList();
            await _emailService.SendEmailAsync(
                toAddresses,
                request.Subject,
                request.Body,
                cancellationToken);

            alert.MarkAsSent();

            // Update or create cooldown
            if (request.DeviceId.HasValue && !string.IsNullOrEmpty(request.AlertType))
            {
                var cooldown = await _context.AlertCooldowns
                    .FirstOrDefaultAsync(c =>
                        c.DeviceId == request.DeviceId.Value &&
                        c.AlertType == request.AlertType,
                        cancellationToken);

                if (cooldown is not null)
                {
                    cooldown.ResetCooldown();
                }
                else
                {
                    cooldown = AlertCooldown.Create(request.DeviceId.Value, request.AlertType);
                    _context.AlertCooldowns.Add(cooldown);
                }
            }

            _logger.LogInformation(
                "Alert sent successfully. Subject: {Subject}, Recipients: {RecipientCount}",
                request.Subject, recipients.Count);
        }
        catch (Exception ex)
        {
            alert.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Failed to send alert: {Subject}", request.Subject);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
