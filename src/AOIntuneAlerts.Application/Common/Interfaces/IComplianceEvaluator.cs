using AOIntuneAlerts.Domain.Aggregates.Device;

namespace AOIntuneAlerts.Application.Common.Interfaces;

public interface IComplianceEvaluator
{
    Task EvaluateDeviceComplianceAsync(Device device, CancellationToken cancellationToken = default);
    Task EvaluateAllDevicesAsync(CancellationToken cancellationToken = default);
}
