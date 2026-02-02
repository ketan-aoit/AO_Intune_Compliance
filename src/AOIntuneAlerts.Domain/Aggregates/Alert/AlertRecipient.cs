using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.ValueObjects;

namespace AOIntuneAlerts.Domain.Aggregates.Alert;

public class AlertRecipient : Entity<Guid>
{
    public EmailAddress Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public AlertSeverity MinimumSeverity { get; private set; }

    private AlertRecipient() { }

    private AlertRecipient(
        Guid id,
        EmailAddress email,
        string displayName,
        AlertSeverity minimumSeverity)
        : base(id)
    {
        Email = email;
        DisplayName = displayName;
        MinimumSeverity = minimumSeverity;
        IsEnabled = true;
    }

    public static AlertRecipient Create(
        string email,
        string displayName,
        AlertSeverity minimumSeverity = AlertSeverity.Warning)
    {
        var emailAddress = EmailAddress.Create(email);
        return new AlertRecipient(Guid.NewGuid(), emailAddress, displayName, minimumSeverity);
    }

    public void Update(string displayName, AlertSeverity minimumSeverity)
    {
        DisplayName = displayName;
        MinimumSeverity = minimumSeverity;
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

    public bool ShouldReceiveAlert(AlertSeverity alertSeverity)
    {
        return IsEnabled && alertSeverity >= MinimumSeverity;
    }
}
