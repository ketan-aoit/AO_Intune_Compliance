using AOIntuneAlerts.Domain.Common;

namespace AOIntuneAlerts.Domain.Aggregates.Alert;

public class AlertCooldown : Entity<Guid>
{
    public Guid DeviceId { get; private set; }
    public string AlertType { get; private set; } = string.Empty;
    public DateTime LastAlertSent { get; private set; }
    public DateTime CooldownExpiresAt { get; private set; }

    private AlertCooldown() { }

    private AlertCooldown(
        Guid id,
        Guid deviceId,
        string alertType,
        int cooldownDays)
        : base(id)
    {
        DeviceId = deviceId;
        AlertType = alertType;
        LastAlertSent = DateTime.UtcNow;
        CooldownExpiresAt = DateTime.UtcNow.AddDays(cooldownDays);
    }

    public static AlertCooldown Create(Guid deviceId, string alertType, int cooldownDays = 7)
    {
        return new AlertCooldown(Guid.NewGuid(), deviceId, alertType, cooldownDays);
    }

    public bool IsInCooldown()
    {
        return DateTime.UtcNow < CooldownExpiresAt;
    }

    public void ResetCooldown(int cooldownDays = 7)
    {
        LastAlertSent = DateTime.UtcNow;
        CooldownExpiresAt = DateTime.UtcNow.AddDays(cooldownDays);
        UpdatedAt = DateTime.UtcNow;
    }
}
