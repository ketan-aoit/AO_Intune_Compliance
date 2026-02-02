using AOIntuneAlerts.Application.Common.Models;

namespace AOIntuneAlerts.Application.Common.Interfaces;

public interface IIntuneGraphService
{
    Task<IEnumerable<IntuneDeviceDto>> GetManagedDevicesAsync(CancellationToken cancellationToken = default);
    Task<IntuneDeviceDto?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default);
}
