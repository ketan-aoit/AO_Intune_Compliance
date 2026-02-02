using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.ValueObjects;

namespace AOIntuneAlerts.Domain.Aggregates.Device;

public class DeviceBrowser : Entity<Guid>
{
    public Guid DeviceId { get; private set; }
    public BrowserInfo BrowserInfo { get; private set; } = null!;
    public bool IsCompliant { get; private set; }
    public DateTime? LastCheckedAt { get; private set; }

    private DeviceBrowser() { }

    private DeviceBrowser(Guid id, Guid deviceId, BrowserInfo browserInfo)
        : base(id)
    {
        DeviceId = deviceId;
        BrowserInfo = browserInfo;
        IsCompliant = true;
        LastCheckedAt = DateTime.UtcNow;
    }

    public static DeviceBrowser Create(Guid deviceId, BrowserInfo browserInfo)
    {
        return new DeviceBrowser(Guid.NewGuid(), deviceId, browserInfo);
    }

    public void SetCompliance(bool isCompliant)
    {
        IsCompliant = isCompliant;
        LastCheckedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
