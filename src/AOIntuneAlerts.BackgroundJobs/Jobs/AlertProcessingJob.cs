using AOIntuneAlerts.Application.Alerts.Commands;
using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.BackgroundJobs.Jobs;

public class AlertProcessingJob
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AlertProcessingJob> _logger;

    public AlertProcessingJob(
        IMediator mediator,
        IApplicationDbContext context,
        ILogger<AlertProcessingJob> logger)
    {
        _mediator = mediator;
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting alert processing job");

        try
        {
            // Get devices approaching end of support
            var devicesApproachingEos = await _context.Devices
                .Where(d => d.EndOfSupportDate != null)
                .Where(d => d.ComplianceState == ComplianceState.ApproachingEndOfSupport ||
                           d.ComplianceState == ComplianceState.NonCompliant)
                .ToListAsync();

            var alertsSent = 0;

            foreach (var device in devicesApproachingEos)
            {
                if (device.EndOfSupportDate is null)
                    continue;

                var daysUntilEos = (device.EndOfSupportDate.Value.Date - DateTime.UtcNow.Date).Days;

                // Determine severity and alert type based on days remaining
                var (severity, alertType, shouldAlert) = daysUntilEos switch
                {
                    <= 0 => (AlertSeverity.Critical, "eos-expired", true),
                    <= 30 => (AlertSeverity.Critical, "eos-30-days", true),
                    <= 60 => (AlertSeverity.Warning, "eos-60-days", true),
                    <= 90 => (AlertSeverity.Information, "eos-90-days", true),
                    _ => (AlertSeverity.Information, string.Empty, false)
                };

                if (!shouldAlert)
                    continue;

                var subject = daysUntilEos <= 0
                    ? $"CRITICAL: {device.DeviceName} has reached end of support"
                    : $"Device {device.DeviceName} - End of Support in {daysUntilEos} days";

                var body = GenerateAlertBody(device.DeviceName, device.UserDisplayName,
                    device.OperatingSystem.ToString(), device.EndOfSupportDate.Value, daysUntilEos);

                var command = new SendAlertCommand
                {
                    Subject = subject,
                    Body = body,
                    Severity = severity,
                    DeviceId = device.Id,
                    AlertType = alertType
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    alertsSent++;
                }
            }

            // Check for non-compliant devices without EOS issues
            var nonCompliantDevices = await _context.Devices
                .Include(d => d.ComplianceIssues)
                .Where(d => d.ComplianceState == ComplianceState.NonCompliant)
                .Where(d => d.EndOfSupportDate == null)
                .Where(d => d.ComplianceIssues.Any())
                .ToListAsync();

            foreach (var device in nonCompliantDevices)
            {
                var criticalIssues = device.ComplianceIssues
                    .Where(i => i.Severity == AlertSeverity.Critical)
                    .ToList();

                if (!criticalIssues.Any())
                    continue;

                var subject = $"Compliance Alert: {device.DeviceName} has {criticalIssues.Count} critical issues";
                var body = GenerateComplianceAlertBody(device.DeviceName, device.UserDisplayName, criticalIssues);

                var command = new SendAlertCommand
                {
                    Subject = subject,
                    Body = body,
                    Severity = AlertSeverity.Critical,
                    DeviceId = device.Id,
                    AlertType = "compliance-critical"
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    alertsSent++;
                }
            }

            _logger.LogInformation("Alert processing completed. Alerts sent: {AlertsSent}", alertsSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during alert processing job");
            throw;
        }
    }

    private static string GenerateAlertBody(string deviceName, string? userName,
        string operatingSystem, DateTime endOfSupportDate, int daysRemaining)
    {
        var urgency = daysRemaining <= 0 ? "has expired" :
                     daysRemaining <= 30 ? "is critical" :
                     daysRemaining <= 60 ? "requires attention" : "is approaching";

        return $@"
<html>
<body style='font-family: Segoe UI, Arial, sans-serif;'>
<h2>Device End of Support Alert</h2>
<p>The following device {urgency}:</p>
<table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Device Name</td><td style='padding: 8px; border: 1px solid #ddd;'>{deviceName}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Assigned User</td><td style='padding: 8px; border: 1px solid #ddd;'>{userName ?? "Not assigned"}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Operating System</td><td style='padding: 8px; border: 1px solid #ddd;'>{operatingSystem}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>End of Support Date</td><td style='padding: 8px; border: 1px solid #ddd;'>{endOfSupportDate:MMMM dd, yyyy}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Days Remaining</td><td style='padding: 8px; border: 1px solid #ddd; color: {(daysRemaining <= 30 ? "red" : daysRemaining <= 60 ? "orange" : "green")};'>{(daysRemaining <= 0 ? "EXPIRED" : daysRemaining.ToString())}</td></tr>
</table>
<p style='margin-top: 20px;'>Please take action to update or replace this device to maintain Cyber Essentials compliance.</p>
<p style='color: #666; font-size: 12px;'>This is an automated message from the Intune Compliance Portal.</p>
</body>
</html>";
    }

    private static string GenerateComplianceAlertBody(string deviceName, string? userName,
        IEnumerable<Domain.Aggregates.Device.DeviceComplianceIssue> issues)
    {
        var issueRows = string.Join("", issues.Select(i =>
            $"<tr><td style='padding: 8px; border: 1px solid #ddd;'>{i.RuleName}</td><td style='padding: 8px; border: 1px solid #ddd;'>{i.Description}</td></tr>"));

        return $@"
<html>
<body style='font-family: Segoe UI, Arial, sans-serif;'>
<h2>Device Compliance Alert</h2>
<p>The following device has critical compliance issues:</p>
<table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Device Name</td><td style='padding: 8px; border: 1px solid #ddd;'>{deviceName}</td></tr>
<tr><td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Assigned User</td><td style='padding: 8px; border: 1px solid #ddd;'>{userName ?? "Not assigned"}</td></tr>
</table>
<h3>Compliance Issues</h3>
<table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
<tr style='background-color: #f2f2f2;'><th style='padding: 8px; border: 1px solid #ddd; text-align: left;'>Issue</th><th style='padding: 8px; border: 1px solid #ddd; text-align: left;'>Description</th></tr>
{issueRows}
</table>
<p style='margin-top: 20px;'>Please take action to resolve these issues to maintain Cyber Essentials compliance.</p>
<p style='color: #666; font-size: 12px;'>This is an automated message from the Intune Compliance Portal.</p>
</body>
</html>";
    }
}
