namespace AOIntuneAlerts.Application.Common.Models;

public class IntuneDeviceDto
{
    public string Id { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? UserPrincipalName { get; set; }
    public string? UserDisplayName { get; set; }
    public string? OperatingSystem { get; set; }
    public string? OsVersion { get; set; }
    public string? DeviceType { get; set; }
    public string? ComplianceState { get; set; }
    public DateTime? LastSyncDateTime { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsManaged { get; set; }
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? Manufacturer { get; set; }
}
