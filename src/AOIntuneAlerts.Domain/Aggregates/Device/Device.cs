using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.Events;
using AOIntuneAlerts.Domain.ValueObjects;

namespace AOIntuneAlerts.Domain.Aggregates.Device;

public class Device : AggregateRoot<Guid>
{
    public string IntuneDeviceId { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string? UserPrincipalName { get; private set; }
    public string? UserDisplayName { get; private set; }
    public DeviceType DeviceType { get; private set; }
    public OperatingSystemInfo OperatingSystem { get; private set; } = null!;
    public ComplianceState ComplianceState { get; private set; }
    public ComplianceState IntuneComplianceState { get; private set; }
    public DateTime? LastSyncDateTime { get; private set; }
    public DateTime? LastComplianceEvaluationDate { get; private set; }
    public DateTime? EndOfSupportDate { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsManaged { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? Model { get; private set; }
    public string? Manufacturer { get; private set; }

    private readonly List<DeviceComplianceIssue> _complianceIssues = new();
    public IReadOnlyCollection<DeviceComplianceIssue> ComplianceIssues => _complianceIssues.AsReadOnly();

    private readonly List<DeviceBrowser> _browsers = new();
    public IReadOnlyCollection<DeviceBrowser> Browsers => _browsers.AsReadOnly();

    private Device() { }

    private Device(
        Guid id,
        string intuneDeviceId,
        string deviceName,
        OperatingSystemInfo operatingSystem,
        DeviceType deviceType)
        : base(id)
    {
        IntuneDeviceId = intuneDeviceId;
        DeviceName = deviceName;
        OperatingSystem = operatingSystem;
        DeviceType = deviceType;
        ComplianceState = ComplianceState.Unknown;
        IntuneComplianceState = ComplianceState.Unknown;
        IsManaged = true;
    }

    public static Device Create(
        string intuneDeviceId,
        string deviceName,
        OperatingSystemInfo operatingSystem,
        DeviceType deviceType = DeviceType.Unknown)
    {
        if (string.IsNullOrWhiteSpace(intuneDeviceId))
            throw new ArgumentException("Intune device ID is required", nameof(intuneDeviceId));

        if (string.IsNullOrWhiteSpace(deviceName))
            throw new ArgumentException("Device name is required", nameof(deviceName));

        var device = new Device(Guid.NewGuid(), intuneDeviceId, deviceName, operatingSystem, deviceType);
        device.AddDomainEvent(new DeviceCreatedEvent(device.Id, intuneDeviceId, deviceName));

        return device;
    }

    public void UpdateFromIntune(
        string deviceName,
        string? userPrincipalName,
        string? userDisplayName,
        OperatingSystemInfo operatingSystem,
        DeviceType deviceType,
        ComplianceState intuneComplianceState,
        DateTime? lastSyncDateTime,
        bool isEncrypted,
        bool isManaged,
        string? serialNumber,
        string? model,
        string? manufacturer)
    {
        DeviceName = deviceName;
        UserPrincipalName = userPrincipalName;
        UserDisplayName = userDisplayName;
        OperatingSystem = operatingSystem;
        DeviceType = deviceType;
        IntuneComplianceState = intuneComplianceState;
        LastSyncDateTime = lastSyncDateTime;
        IsEncrypted = isEncrypted;
        IsManaged = isManaged;
        SerialNumber = serialNumber;
        Model = model;
        Manufacturer = manufacturer;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DeviceUpdatedEvent(Id, IntuneDeviceId));
    }

    public void SetComplianceState(ComplianceState state, DateTime? endOfSupportDate = null)
    {
        var previousState = ComplianceState;
        ComplianceState = state;
        EndOfSupportDate = endOfSupportDate;
        LastComplianceEvaluationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (previousState != state)
        {
            AddDomainEvent(new DeviceComplianceStateChangedEvent(Id, IntuneDeviceId, previousState, state));
        }
    }

    public void AddComplianceIssue(string ruleId, string ruleName, string description, AlertSeverity severity)
    {
        var issue = DeviceComplianceIssue.Create(Id, ruleId, ruleName, description, severity);
        _complianceIssues.Add(issue);
    }

    public void ClearComplianceIssues()
    {
        _complianceIssues.Clear();
    }

    public void UpdateBrowsers(IEnumerable<BrowserInfo> browsers)
    {
        _browsers.Clear();
        foreach (var browser in browsers)
        {
            _browsers.Add(DeviceBrowser.Create(Id, browser));
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
