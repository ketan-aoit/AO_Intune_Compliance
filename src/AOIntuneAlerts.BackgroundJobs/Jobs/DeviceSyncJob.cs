using AOIntuneAlerts.Application.Devices.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.BackgroundJobs.Jobs;

public class DeviceSyncJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeviceSyncJob> _logger;

    public DeviceSyncJob(IMediator mediator, ILogger<DeviceSyncJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting device sync job");

        try
        {
            var result = await _mediator.Send(new SyncDevicesCommand());

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Device sync completed successfully. Synced: {Synced}, Created: {Created}, Updated: {Updated}",
                    result.Value!.DevicesSynced,
                    result.Value.DevicesCreated,
                    result.Value.DevicesUpdated);
            }
            else
            {
                _logger.LogError("Device sync failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during device sync job");
            throw;
        }
    }
}
