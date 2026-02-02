namespace AOIntuneAlerts.Domain.Events;

public class DeviceUpdatedEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public string IntuneDeviceId { get; }

    public DeviceUpdatedEvent(Guid deviceId, string intuneDeviceId)
    {
        DeviceId = deviceId;
        IntuneDeviceId = intuneDeviceId;
    }
}
