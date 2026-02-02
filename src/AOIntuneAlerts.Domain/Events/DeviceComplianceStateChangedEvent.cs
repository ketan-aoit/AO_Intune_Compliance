using AOIntuneAlerts.Domain.Enums;

namespace AOIntuneAlerts.Domain.Events;

public class DeviceComplianceStateChangedEvent : DomainEvent
{
    public Guid DeviceId { get; }
    public string IntuneDeviceId { get; }
    public ComplianceState PreviousState { get; }
    public ComplianceState NewState { get; }

    public DeviceComplianceStateChangedEvent(
        Guid deviceId,
        string intuneDeviceId,
        ComplianceState previousState,
        ComplianceState newState)
    {
        DeviceId = deviceId;
        IntuneDeviceId = intuneDeviceId;
        PreviousState = previousState;
        NewState = newState;
    }
}
