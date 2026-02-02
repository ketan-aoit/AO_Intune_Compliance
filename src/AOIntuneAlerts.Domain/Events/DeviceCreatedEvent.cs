namespace AOIntuneAlerts.Domain.Events;

public class DeviceCreatedEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public string IntuneDeviceId { get; }
    public string DeviceName { get; }

    public DeviceCreatedEvent(Guid deviceId, string intuneDeviceId, string deviceName)
    {
        DeviceId = deviceId;
        IntuneDeviceId = intuneDeviceId;
        DeviceName = deviceName;
    }
}
