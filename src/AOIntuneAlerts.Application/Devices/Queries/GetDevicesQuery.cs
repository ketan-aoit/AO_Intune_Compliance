using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Devices.Queries;

public record GetDevicesQuery : IRequest<PaginatedList<DeviceDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public ComplianceState? ComplianceState { get; init; }
    public OperatingSystemType? OperatingSystemType { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public record DeviceDto
{
    public Guid Id { get; init; }
    public string IntuneDeviceId { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string? UserPrincipalName { get; init; }
    public string? UserDisplayName { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string OperatingSystem { get; init; } = string.Empty;
    public string OsVersion { get; init; } = string.Empty;
    public string ComplianceState { get; init; } = string.Empty;
    public string IntuneComplianceState { get; init; } = string.Empty;
    public DateTime? LastSyncDateTime { get; init; }
    public DateTime? LastComplianceEvaluationDate { get; init; }
    public DateTime? EndOfSupportDate { get; init; }
    public bool IsEncrypted { get; init; }
    public int ComplianceIssueCount { get; init; }
}

public class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, PaginatedList<DeviceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDevicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<DeviceDto>> Handle(
        GetDevicesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Devices
            .Include(d => d.ComplianceIssues)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DeviceName.ToLower().Contains(searchTerm) ||
                (d.UserPrincipalName != null && d.UserPrincipalName.ToLower().Contains(searchTerm)) ||
                (d.UserDisplayName != null && d.UserDisplayName.ToLower().Contains(searchTerm)) ||
                (d.SerialNumber != null && d.SerialNumber.ToLower().Contains(searchTerm)));
        }

        if (request.ComplianceState.HasValue)
        {
            // Filter by effective compliance state: internal if evaluated, otherwise Intune's state
            query = query.Where(d =>
                d.LastComplianceEvaluationDate.HasValue
                    ? d.ComplianceState == request.ComplianceState.Value
                    : d.IntuneComplianceState == request.ComplianceState.Value);
        }

        if (request.OperatingSystemType.HasValue)
        {
            query = query.Where(d => d.OperatingSystem.Type == request.OperatingSystemType.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(d => d.DeviceName)
                : query.OrderBy(d => d.DeviceName),
            "user" => request.SortDescending
                ? query.OrderByDescending(d => d.UserDisplayName)
                : query.OrderBy(d => d.UserDisplayName),
            "lastsync" => request.SortDescending
                ? query.OrderByDescending(d => d.LastSyncDateTime)
                : query.OrderBy(d => d.LastSyncDateTime),
            "compliance" => request.SortDescending
                ? query.OrderByDescending(d => d.ComplianceState)
                : query.OrderBy(d => d.ComplianceState),
            _ => query.OrderBy(d => d.DeviceName)
        };

        var dtoQuery = query.Select(d => new DeviceDto
        {
            Id = d.Id,
            IntuneDeviceId = d.IntuneDeviceId,
            DeviceName = d.DeviceName,
            UserPrincipalName = d.UserPrincipalName,
            UserDisplayName = d.UserDisplayName,
            DeviceType = d.DeviceType.ToString(),
            OperatingSystem = d.OperatingSystem.Name,
            OsVersion = d.OperatingSystem.Version.Major + "." + d.OperatingSystem.Version.Minor + "." + d.OperatingSystem.Version.Patch,
            // Use internal compliance state if evaluated, otherwise fall back to Intune's state
            ComplianceState = d.LastComplianceEvaluationDate.HasValue
                ? d.ComplianceState.ToString()
                : d.IntuneComplianceState.ToString(),
            IntuneComplianceState = d.IntuneComplianceState.ToString(),
            LastSyncDateTime = d.LastSyncDateTime,
            LastComplianceEvaluationDate = d.LastComplianceEvaluationDate,
            EndOfSupportDate = d.EndOfSupportDate,
            IsEncrypted = d.IsEncrypted,
            ComplianceIssueCount = d.ComplianceIssues.Count
        });

        return await PaginatedList<DeviceDto>.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
