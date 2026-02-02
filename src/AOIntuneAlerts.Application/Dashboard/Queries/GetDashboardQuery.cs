using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;

public record DashboardDto
{
    public ComplianceOverviewDto ComplianceOverview { get; init; } = new();
    public List<RecentAlertDto> RecentAlerts { get; init; } = new();
    public List<DeviceAtRiskDto> DevicesAtRisk { get; init; } = new();
    public List<ComplianceTrendDto> ComplianceTrend { get; init; } = new();
    public DateTime? LastSyncTime { get; init; }
    public DateTime? LastComplianceEvaluation { get; init; }
}

public record ComplianceOverviewDto
{
    public int TotalDevices { get; init; }
    public int CompliantDevices { get; init; }
    public int NonCompliantDevices { get; init; }
    public int ApproachingEosDevices { get; init; }
    public int UnknownDevices { get; init; }
    public double CompliancePercentage { get; init; }
}

public record RecentAlertDto
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}

public record DeviceAtRiskDto
{
    public Guid Id { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string? UserDisplayName { get; init; }
    public string ComplianceState { get; init; } = string.Empty;
    public DateTime? EndOfSupportDate { get; init; }
    public int IssueCount { get; init; }
}

public record ComplianceTrendDto
{
    public DateTime Date { get; init; }
    public double CompliancePercentage { get; init; }
}

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await _context.Devices
            .Include(d => d.ComplianceIssues)
            .ToListAsync(cancellationToken);

        // Use effective compliance state: internal if evaluated, otherwise Intune's state
        ComplianceState GetEffectiveState(Domain.Aggregates.Device.Device d) =>
            d.LastComplianceEvaluationDate.HasValue ? d.ComplianceState : d.IntuneComplianceState;

        var totalDevices = devices.Count;
        var compliantDevices = devices.Count(d => GetEffectiveState(d) == ComplianceState.Compliant);
        var nonCompliantDevices = devices.Count(d => GetEffectiveState(d) == ComplianceState.NonCompliant);
        var approachingEosDevices = devices.Count(d => GetEffectiveState(d) == ComplianceState.ApproachingEndOfSupport);
        var unknownDevices = devices.Count(d => GetEffectiveState(d) == ComplianceState.Unknown);

        var compliancePercentage = totalDevices > 0
            ? Math.Round((double)compliantDevices / totalDevices * 100, 1)
            : 0;

        // Get recent alerts
        var recentAlerts = await _context.Alerts
            .Where(a => a.WasSent)
            .OrderByDescending(a => a.SentAt)
            .Take(5)
            .Select(a => new RecentAlertDto
            {
                Id = a.Id,
                Subject = a.Subject,
                Severity = a.Severity.ToString(),
                SentAt = a.SentAt
            })
            .ToListAsync(cancellationToken);

        // Get devices at risk (non-compliant or approaching EOS)
        var devicesAtRisk = devices
            .Where(d => GetEffectiveState(d) == ComplianceState.NonCompliant ||
                       GetEffectiveState(d) == ComplianceState.ApproachingEndOfSupport ||
                       GetEffectiveState(d) == ComplianceState.InGracePeriod ||
                       GetEffectiveState(d) == ComplianceState.Error ||
                       GetEffectiveState(d) == ComplianceState.Conflict)
            .OrderByDescending(d => d.ComplianceIssues.Count)
            .ThenBy(d => d.EndOfSupportDate)
            .Take(10)
            .Select(d => new DeviceAtRiskDto
            {
                Id = d.Id,
                DeviceName = d.DeviceName,
                UserDisplayName = d.UserDisplayName,
                ComplianceState = GetEffectiveState(d).ToString(),
                EndOfSupportDate = d.EndOfSupportDate,
                IssueCount = d.ComplianceIssues.Count
            })
            .ToList();

        // Get last sync and evaluation times (handle empty collections)
        var lastSyncTime = devices.Count > 0 ? devices.Max(d => d.LastSyncDateTime) : null;
        var lastComplianceEvaluation = devices.Count > 0 ? devices.Max(d => d.LastComplianceEvaluationDate) : null;

        return new DashboardDto
        {
            ComplianceOverview = new ComplianceOverviewDto
            {
                TotalDevices = totalDevices,
                CompliantDevices = compliantDevices,
                NonCompliantDevices = nonCompliantDevices,
                ApproachingEosDevices = approachingEosDevices,
                UnknownDevices = unknownDevices,
                CompliancePercentage = compliancePercentage
            },
            RecentAlerts = recentAlerts,
            DevicesAtRisk = devicesAtRisk,
            LastSyncTime = lastSyncTime,
            LastComplianceEvaluation = lastComplianceEvaluation
        };
    }
}
