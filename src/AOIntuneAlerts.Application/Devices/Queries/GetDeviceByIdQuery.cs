using AOIntuneAlerts.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Devices.Queries;

public record GetDeviceByIdQuery(Guid Id) : IRequest<DeviceDetailDto?>;

public record DeviceDetailDto
{
    public Guid Id { get; init; }
    public string IntuneDeviceId { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string? UserPrincipalName { get; init; }
    public string? UserDisplayName { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string OperatingSystem { get; init; } = string.Empty;
    public string OsVersion { get; init; } = string.Empty;
    public string? OsEdition { get; init; }
    public string ComplianceState { get; init; } = string.Empty;
    public string IntuneComplianceState { get; init; } = string.Empty;
    public DateTime? LastSyncDateTime { get; init; }
    public DateTime? LastComplianceEvaluationDate { get; init; }
    public DateTime? EndOfSupportDate { get; init; }
    public bool IsEncrypted { get; init; }
    public bool IsManaged { get; init; }
    public string? SerialNumber { get; init; }
    public string? Model { get; init; }
    public string? Manufacturer { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ComplianceIssueDto> ComplianceIssues { get; init; } = new();
    public List<BrowserDto> Browsers { get; init; } = new();
}

public record ComplianceIssueDto
{
    public Guid Id { get; init; }
    public string RuleId { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime DetectedAt { get; init; }
}

public record BrowserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public bool IsCompliant { get; init; }
}

public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, DeviceDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDeviceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceDetailDto?> Handle(
        GetDeviceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(d => d.ComplianceIssues)
            .Include(d => d.Browsers)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (device is null)
            return null;

        return new DeviceDetailDto
        {
            Id = device.Id,
            IntuneDeviceId = device.IntuneDeviceId,
            DeviceName = device.DeviceName,
            UserPrincipalName = device.UserPrincipalName,
            UserDisplayName = device.UserDisplayName,
            DeviceType = device.DeviceType.ToString(),
            OperatingSystem = device.OperatingSystem.Name,
            OsVersion = device.OperatingSystem.Version.ToString(),
            OsEdition = device.OperatingSystem.Edition,
            // Use internal compliance state if evaluated, otherwise fall back to Intune's state
            ComplianceState = device.LastComplianceEvaluationDate.HasValue
                ? device.ComplianceState.ToString()
                : device.IntuneComplianceState.ToString(),
            IntuneComplianceState = device.IntuneComplianceState.ToString(),
            LastSyncDateTime = device.LastSyncDateTime,
            LastComplianceEvaluationDate = device.LastComplianceEvaluationDate,
            EndOfSupportDate = device.EndOfSupportDate,
            IsEncrypted = device.IsEncrypted,
            IsManaged = device.IsManaged,
            SerialNumber = device.SerialNumber,
            Model = device.Model,
            Manufacturer = device.Manufacturer,
            CreatedAt = device.CreatedAt,
            UpdatedAt = device.UpdatedAt,
            ComplianceIssues = device.ComplianceIssues.Select(i => new ComplianceIssueDto
            {
                Id = i.Id,
                RuleId = i.RuleId,
                RuleName = i.RuleName,
                Description = i.Description,
                Severity = i.Severity.ToString(),
                DetectedAt = i.DetectedAt
            }).ToList(),
            Browsers = device.Browsers.Select(b => new BrowserDto
            {
                Id = b.Id,
                Name = b.BrowserInfo.Name,
                Version = b.BrowserInfo.Version.ToString(),
                IsCompliant = b.IsCompliant
            }).ToList()
        };
    }
}
