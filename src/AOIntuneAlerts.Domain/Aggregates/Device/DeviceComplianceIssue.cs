using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.Aggregates.Device;

public class DeviceComplianceIssue : Entity<Guid>
{
    public Guid DeviceId { get; private set; }
    public string RuleId { get; private set; } = string.Empty;
    public string RuleName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public DateTime DetectedAt { get; private set; }

    private DeviceComplianceIssue() { }

    private DeviceComplianceIssue(
        Guid id,
        Guid deviceId,
        string ruleId,
        string ruleName,
        string description,
        AlertSeverity severity)
        : base(id)
    {
        DeviceId = deviceId;
        RuleId = ruleId;
        RuleName = ruleName;
        Description = description;
        Severity = severity;
        DetectedAt = DateTime.UtcNow;
    }

    public static DeviceComplianceIssue Create(
        Guid deviceId,
        string ruleId,
        string ruleName,
        string description,
        AlertSeverity severity)
    {
        return new DeviceComplianceIssue(Guid.NewGuid(), deviceId, ruleId, ruleName, description, severity);
    }
}
