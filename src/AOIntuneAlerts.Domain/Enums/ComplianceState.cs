namespace AOIntuneAlerts.Domain.Enums;

public enum ComplianceState
{
    Unknown = 0,
    Compliant = 1,
    NonCompliant = 2,
    ApproachingEndOfSupport = 3,
    InGracePeriod = 4,
    ConfigManager = 5,
    Conflict = 6,
    Error = 7
}
