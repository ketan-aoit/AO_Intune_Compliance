using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.ValueObjects;

namespace AOIntuneAlerts.Domain.Aggregates.ComplianceRule;

public class VendorSupportDate : Entity<Guid>
{
    public OperatingSystemType OperatingSystemType { get; private set; }
    public string VersionPattern { get; private set; } = string.Empty;
    public SemVer MinimumVersion { get; private set; } = null!;
    public DateTime EndOfSupportDate { get; private set; }
    public string? Notes { get; private set; }

    private VendorSupportDate() { }

    private VendorSupportDate(
        Guid id,
        OperatingSystemType osType,
        string versionPattern,
        SemVer minimumVersion,
        DateTime endOfSupportDate,
        string? notes)
        : base(id)
    {
        OperatingSystemType = osType;
        VersionPattern = versionPattern;
        MinimumVersion = minimumVersion;
        EndOfSupportDate = endOfSupportDate;
        Notes = notes;
    }

    public static VendorSupportDate Create(
        OperatingSystemType osType,
        string versionPattern,
        SemVer minimumVersion,
        DateTime endOfSupportDate,
        string? notes = null)
    {
        return new VendorSupportDate(
            Guid.NewGuid(),
            osType,
            versionPattern,
            minimumVersion,
            endOfSupportDate,
            notes);
    }

    public void Update(DateTime endOfSupportDate, string? notes)
    {
        EndOfSupportDate = endOfSupportDate;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public int DaysUntilEndOfSupport()
    {
        return (EndOfSupportDate.Date - DateTime.UtcNow.Date).Days;
    }

    public bool IsApproachingEndOfSupport(int warningDays = 90)
    {
        var daysRemaining = DaysUntilEndOfSupport();
        return daysRemaining > 0 && daysRemaining <= warningDays;
    }

    public bool IsEndOfSupport()
    {
        return DateTime.UtcNow.Date >= EndOfSupportDate.Date;
    }
}
