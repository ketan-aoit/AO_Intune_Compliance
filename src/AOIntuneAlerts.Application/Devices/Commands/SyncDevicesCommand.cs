using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Aggregates.Device;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.Application.Devices.Commands;

public record SyncDevicesCommand : IRequest<Result<SyncDevicesResult>>;

public record SyncDevicesResult(int DevicesSynced, int DevicesCreated, int DevicesUpdated);

public class SyncDevicesCommandHandler : IRequestHandler<SyncDevicesCommand, Result<SyncDevicesResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIntuneGraphService _intuneService;
    private readonly ILogger<SyncDevicesCommandHandler> _logger;

    public SyncDevicesCommandHandler(
        IApplicationDbContext context,
        IIntuneGraphService intuneService,
        ILogger<SyncDevicesCommandHandler> logger)
    {
        _context = context;
        _intuneService = intuneService;
        _logger = logger;
    }

    public async Task<Result<SyncDevicesResult>> Handle(
        SyncDevicesCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var intuneDevices = await _intuneService.GetManagedDevicesAsync(cancellationToken);
            var existingDevices = await _context.Devices
                .ToDictionaryAsync(d => d.IntuneDeviceId, cancellationToken);

            int created = 0, updated = 0;

            foreach (var intuneDevice in intuneDevices)
            {
                var osInfo = OperatingSystemInfo.CreateFromString(
                    $"{intuneDevice.OperatingSystem} {intuneDevice.OsVersion}");

                var deviceType = ParseDeviceType(intuneDevice.DeviceType);
                var complianceState = ParseComplianceState(intuneDevice.ComplianceState);

                if (existingDevices.TryGetValue(intuneDevice.Id, out var device))
                {
                    device.UpdateFromIntune(
                        intuneDevice.DeviceName,
                        intuneDevice.UserPrincipalName,
                        intuneDevice.UserDisplayName,
                        osInfo,
                        deviceType,
                        complianceState,
                        intuneDevice.LastSyncDateTime,
                        intuneDevice.IsEncrypted,
                        intuneDevice.IsManaged,
                        intuneDevice.SerialNumber,
                        intuneDevice.Model,
                        intuneDevice.Manufacturer);
                    updated++;
                }
                else
                {
                    device = Device.Create(
                        intuneDevice.Id,
                        intuneDevice.DeviceName,
                        osInfo,
                        deviceType);

                    device.UpdateFromIntune(
                        intuneDevice.DeviceName,
                        intuneDevice.UserPrincipalName,
                        intuneDevice.UserDisplayName,
                        osInfo,
                        deviceType,
                        complianceState,
                        intuneDevice.LastSyncDateTime,
                        intuneDevice.IsEncrypted,
                        intuneDevice.IsManaged,
                        intuneDevice.SerialNumber,
                        intuneDevice.Model,
                        intuneDevice.Manufacturer);

                    _context.Devices.Add(device);
                    created++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Device sync completed. Created: {Created}, Updated: {Updated}",
                created, updated);

            return Result.Success(new SyncDevicesResult(created + updated, created, updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing devices from Intune");
            return Result.Failure<SyncDevicesResult>($"Failed to sync devices: {ex.Message}");
        }
    }

    private static DeviceType ParseDeviceType(string? deviceType)
    {
        if (string.IsNullOrEmpty(deviceType))
            return DeviceType.Unknown;

        return deviceType.ToLowerInvariant() switch
        {
            "desktop" => DeviceType.Desktop,
            "laptop" => DeviceType.Laptop,
            "tablet" => DeviceType.Tablet,
            "phone" or "smartphone" => DeviceType.Phone,
            "virtual" or "vm" => DeviceType.Virtual,
            _ => DeviceType.Unknown
        };
    }

    private static ComplianceState ParseComplianceState(string? complianceState)
    {
        if (string.IsNullOrEmpty(complianceState))
            return ComplianceState.Unknown;

        // Handle all Intune compliance states (case-insensitive)
        return complianceState.ToLowerInvariant() switch
        {
            "compliant" => ComplianceState.Compliant,
            "noncompliant" or "notcompliant" => ComplianceState.NonCompliant,
            "ingraceperiod" or "in_grace_period" => ComplianceState.InGracePeriod,
            "configmanager" or "config_manager" => ComplianceState.ConfigManager,
            "conflict" => ComplianceState.Conflict,
            "error" => ComplianceState.Error,
            "unknown" => ComplianceState.Unknown,
            _ => ComplianceState.Unknown
        };
    }
}
