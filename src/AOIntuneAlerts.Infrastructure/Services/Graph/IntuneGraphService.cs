using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AOIntuneAlerts.Infrastructure.Services.Graph;

public class IntuneGraphService : IIntuneGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<IntuneGraphService> _logger;

    public IntuneGraphService(GraphServiceClient graphClient, ILogger<IntuneGraphService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<IEnumerable<IntuneDeviceDto>> GetManagedDevicesAsync(CancellationToken cancellationToken = default)
    {
        var devices = new List<IntuneDeviceDto>();

        try
        {
            var response = await _graphClient.DeviceManagement.ManagedDevices
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[]
                    {
                        "id",
                        "deviceName",
                        "userPrincipalName",
                        "userDisplayName",
                        "operatingSystem",
                        "osVersion",
                        "complianceState",
                        "lastSyncDateTime",
                        "isEncrypted",
                        "managedDeviceOwnerType",
                        "serialNumber",
                        "model",
                        "manufacturer"
                    };
                }, cancellationToken);

            if (response?.Value is not null)
            {
                var pageIterator = PageIterator<ManagedDevice, ManagedDeviceCollectionResponse>
                    .CreatePageIterator(
                        _graphClient,
                        response,
                        device =>
                        {
                            devices.Add(MapToDto(device));
                            return true;
                        });

                await pageIterator.IterateAsync(cancellationToken);
            }

            _logger.LogInformation("Retrieved {Count} managed devices from Intune", devices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving managed devices from Intune");
            throw;
        }

        return devices;
    }

    public async Task<IntuneDeviceDto?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var device = await _graphClient.DeviceManagement.ManagedDevices[deviceId]
                .GetAsync(cancellationToken: cancellationToken);

            return device is not null ? MapToDto(device) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving device {DeviceId} from Intune", deviceId);
            throw;
        }
    }

    private static IntuneDeviceDto MapToDto(ManagedDevice device)
    {
        return new IntuneDeviceDto
        {
            Id = device.Id ?? string.Empty,
            DeviceName = device.DeviceName ?? "Unknown",
            UserPrincipalName = device.UserPrincipalName,
            UserDisplayName = device.UserDisplayName,
            OperatingSystem = device.OperatingSystem,
            OsVersion = device.OsVersion,
            DeviceType = DetermineDeviceType(device),
            ComplianceState = device.ComplianceState?.ToString(),
            LastSyncDateTime = device.LastSyncDateTime?.UtcDateTime,
            IsEncrypted = device.IsEncrypted ?? false,
            IsManaged = device.ManagedDeviceOwnerType != ManagedDeviceOwnerType.Unknown,
            SerialNumber = device.SerialNumber,
            Model = device.Model,
            Manufacturer = device.Manufacturer
        };
    }

    private static string DetermineDeviceType(ManagedDevice device)
    {
        // Determine device type from OS or other properties
        var os = device.OperatingSystem?.ToLowerInvariant() ?? "";
        var model = device.Model?.ToLowerInvariant() ?? "";

        if (os.Contains("ios") || os.Contains("iphone") || model.Contains("iphone"))
            return "Phone";
        if (os.Contains("ipad") || model.Contains("ipad"))
            return "Tablet";
        if (os.Contains("android"))
        {
            if (model.Contains("tablet") || model.Contains("tab"))
                return "Tablet";
            return "Phone";
        }
        if (os.Contains("macos") || os.Contains("mac os"))
            return "Desktop";
        if (os.Contains("windows"))
        {
            if (model.Contains("surface") && !model.Contains("laptop"))
                return "Tablet";
            if (model.Contains("laptop") || model.Contains("notebook"))
                return "Laptop";
            return "Desktop";
        }

        return "Unknown";
    }
}
