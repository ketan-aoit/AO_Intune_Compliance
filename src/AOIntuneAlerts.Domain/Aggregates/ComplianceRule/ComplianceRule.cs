using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.Aggregates.ComplianceRule;

public class ComplianceRule : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ComplianceRuleType RuleType { get; private set; }
    public bool IsEnabled { get; private set; }
    public AlertSeverity Severity { get; private set; }

    // Rule configuration stored as JSON for flexibility
    public string Configuration { get; private set; } = "{}";

    // Applicable operating systems (null means all)
    public OperatingSystemType? ApplicableOs { get; private set; }

    private ComplianceRule() { }

    private ComplianceRule(
        Guid id,
        string name,
        string description,
        ComplianceRuleType ruleType,
        AlertSeverity severity,
        string configuration,
        OperatingSystemType? applicableOs)
        : base(id)
    {
        Name = name;
        Description = description;
        RuleType = ruleType;
        Severity = severity;
        Configuration = configuration;
        ApplicableOs = applicableOs;
        IsEnabled = true;
    }

    public static ComplianceRule Create(
        string name,
        string description,
        ComplianceRuleType ruleType,
        AlertSeverity severity,
        string configuration,
        OperatingSystemType? applicableOs = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name is required", nameof(name));

        return new ComplianceRule(
            Guid.NewGuid(),
            name,
            description,
            ruleType,
            severity,
            configuration,
            applicableOs);
    }

    public void Update(
        string name,
        string description,
        AlertSeverity severity,
        string configuration,
        OperatingSystemType? applicableOs)
    {
        Name = name;
        Description = description;
        Severity = severity;
        Configuration = configuration;
        ApplicableOs = applicableOs;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
