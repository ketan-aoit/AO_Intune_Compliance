using System.Text.Json;
using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Domain.Aggregates.ComplianceRule;
using AOIntuneAlerts.Domain.Aggregates.Device;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.Infrastructure.Services.Compliance;

public class ComplianceEvaluator : IComplianceEvaluator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ComplianceEvaluator> _logger;

    public ComplianceEvaluator(IApplicationDbContext context, ILogger<ComplianceEvaluator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EvaluateAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _context.Devices
            .Include(d => d.ComplianceIssues)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Starting compliance evaluation for {Count} devices", devices.Count);

        foreach (var device in devices)
        {
            await EvaluateDeviceComplianceAsync(device, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Compliance evaluation completed for {Count} devices", devices.Count);
    }

    public async Task EvaluateDeviceComplianceAsync(Device device, CancellationToken cancellationToken = default)
    {
        device.ClearComplianceIssues();

        var rules = await _context.ComplianceRules
            .Where(r => r.IsEnabled)
            .Where(r => r.ApplicableOs == null || r.ApplicableOs == device.OperatingSystem.Type)
            .ToListAsync(cancellationToken);

        var supportDates = await _context.VendorSupportDates
            .Where(v => v.OperatingSystemType == device.OperatingSystem.Type)
            .ToListAsync(cancellationToken);

        DateTime? endOfSupportDate = null;
        var hasIssues = false;
        var isApproachingEos = false;

        // Check OS version support
        var supportDate = FindApplicableSupportDate(device.OperatingSystem, supportDates);
        if (supportDate is not null)
        {
            endOfSupportDate = supportDate.EndOfSupportDate;

            if (supportDate.IsEndOfSupport())
            {
                device.AddComplianceIssue(
                    "os-eos",
                    "Operating System End of Support",
                    $"{device.OperatingSystem.Name} {device.OperatingSystem.Version} has reached end of support",
                    AlertSeverity.Critical);
                hasIssues = true;
            }
            else if (supportDate.IsApproachingEndOfSupport(90))
            {
                var daysRemaining = supportDate.DaysUntilEndOfSupport();
                var severity = daysRemaining <= 30 ? AlertSeverity.Critical :
                               daysRemaining <= 60 ? AlertSeverity.Warning :
                               AlertSeverity.Information;

                device.AddComplianceIssue(
                    "os-approaching-eos",
                    "Operating System Approaching End of Support",
                    $"{device.OperatingSystem.Name} {device.OperatingSystem.Version} will reach end of support in {daysRemaining} days",
                    severity);
                isApproachingEos = true;
            }
        }

        // Evaluate compliance rules
        foreach (var rule in rules)
        {
            var result = EvaluateRule(device, rule);
            if (!result.IsCompliant)
            {
                device.AddComplianceIssue(
                    rule.Id.ToString(),
                    rule.Name,
                    result.Description,
                    rule.Severity);
                hasIssues = true;
            }
        }

        // Determine overall compliance state
        var complianceState = hasIssues ? ComplianceState.NonCompliant :
                             isApproachingEos ? ComplianceState.ApproachingEndOfSupport :
                             ComplianceState.Compliant;

        device.SetComplianceState(complianceState, endOfSupportDate);

        _logger.LogDebug(
            "Device {DeviceName} evaluated: {ComplianceState}, Issues: {IssueCount}",
            device.DeviceName, complianceState, device.ComplianceIssues.Count);
    }

    private static VendorSupportDate? FindApplicableSupportDate(
        OperatingSystemInfo os,
        IEnumerable<VendorSupportDate> supportDates)
    {
        return supportDates
            .Where(sd => os.Version >= sd.MinimumVersion)
            .OrderByDescending(sd => sd.MinimumVersion)
            .FirstOrDefault();
    }

    private RuleEvaluationResult EvaluateRule(Device device, ComplianceRule rule)
    {
        try
        {
            var config = JsonSerializer.Deserialize<JsonElement>(rule.Configuration);

            return rule.RuleType switch
            {
                ComplianceRuleType.OperatingSystemVersion => EvaluateOsVersionRule(device, config),
                ComplianceRuleType.EncryptionEnabled => EvaluateEncryptionRule(device, config),
                ComplianceRuleType.FirewallEnabled => EvaluateFirewallRule(device, config),
                ComplianceRuleType.SecuritySoftware => EvaluateSecuritySoftwareRule(device, config),
                ComplianceRuleType.BrowserVersion => EvaluateBrowserRule(device, config),
                _ => new RuleEvaluationResult(true, string.Empty)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating rule {RuleName} for device {DeviceName}",
                rule.Name, device.DeviceName);
            return new RuleEvaluationResult(true, string.Empty);
        }
    }

    private static RuleEvaluationResult EvaluateOsVersionRule(Device device, JsonElement config)
    {
        if (!config.TryGetProperty("minimumVersion", out var minVersionElement))
            return new RuleEvaluationResult(true, string.Empty);

        var minVersionStr = minVersionElement.GetString();
        var minVersion = SemVer.TryParse(minVersionStr);

        if (minVersion is null)
            return new RuleEvaluationResult(true, string.Empty);

        if (device.OperatingSystem.Version < minVersion)
        {
            return new RuleEvaluationResult(false,
                $"OS version {device.OperatingSystem.Version} is below minimum required version {minVersion}");
        }

        return new RuleEvaluationResult(true, string.Empty);
    }

    private static RuleEvaluationResult EvaluateEncryptionRule(Device device, JsonElement config)
    {
        if (!device.IsEncrypted)
        {
            return new RuleEvaluationResult(false, "Device encryption is not enabled");
        }

        return new RuleEvaluationResult(true, string.Empty);
    }

    private static RuleEvaluationResult EvaluateFirewallRule(Device device, JsonElement config)
    {
        // Note: Firewall status would need to be retrieved from additional Graph API calls
        // or device configuration profiles. This is a placeholder.
        return new RuleEvaluationResult(true, string.Empty);
    }

    private static RuleEvaluationResult EvaluateSecuritySoftwareRule(Device device, JsonElement config)
    {
        // Note: Security software status would need to be retrieved from Defender for Endpoint
        // or similar APIs. This is a placeholder.
        return new RuleEvaluationResult(true, string.Empty);
    }

    private static RuleEvaluationResult EvaluateBrowserRule(Device device, JsonElement config)
    {
        if (!config.TryGetProperty("browserType", out var browserTypeElement) ||
            !config.TryGetProperty("minimumVersion", out var minVersionElement))
            return new RuleEvaluationResult(true, string.Empty);

        var browserTypeStr = browserTypeElement.GetString();
        var minVersionStr = minVersionElement.GetString();

        if (!Enum.TryParse<BrowserType>(browserTypeStr, true, out var browserType))
            return new RuleEvaluationResult(true, string.Empty);

        var minVersion = SemVer.TryParse(minVersionStr);
        if (minVersion is null)
            return new RuleEvaluationResult(true, string.Empty);

        var browser = device.Browsers.FirstOrDefault(b => b.BrowserInfo.Type == browserType);
        if (browser is null)
            return new RuleEvaluationResult(true, string.Empty);

        if (browser.BrowserInfo.Version < minVersion)
        {
            return new RuleEvaluationResult(false,
                $"{browser.BrowserInfo.Name} version {browser.BrowserInfo.Version} is below minimum required version {minVersion}");
        }

        return new RuleEvaluationResult(true, string.Empty);
    }

    private record RuleEvaluationResult(bool IsCompliant, string Description);
}
