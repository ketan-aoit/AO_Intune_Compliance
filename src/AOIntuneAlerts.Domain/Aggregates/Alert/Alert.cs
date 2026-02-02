using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.Aggregates.Alert;

public class Alert : AggregateRoot<Guid>
{
    public Guid? DeviceId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool WasSent { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<AlertRecipient> _recipients = new();
    public IReadOnlyCollection<AlertRecipient> Recipients => _recipients.AsReadOnly();

    private Alert() { }

    private Alert(
        Guid id,
        Guid? deviceId,
        string subject,
        string body,
        AlertSeverity severity)
        : base(id)
    {
        DeviceId = deviceId;
        Subject = subject;
        Body = body;
        Severity = severity;
        WasSent = false;
    }

    public static Alert Create(
        string subject,
        string body,
        AlertSeverity severity,
        Guid? deviceId = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Alert subject is required", nameof(subject));

        return new Alert(Guid.NewGuid(), deviceId, subject, body, severity);
    }

    public void AddRecipient(AlertRecipient recipient)
    {
        if (_recipients.All(r => r.Email != recipient.Email))
        {
            _recipients.Add(recipient);
        }
    }

    public void MarkAsSent()
    {
        WasSent = true;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        WasSent = false;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }
}
