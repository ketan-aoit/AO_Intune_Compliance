using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Devices.Queries;

public record GetComplianceSummaryQuery : IRequest<ComplianceSummaryDto>;

public record ComplianceSummaryDto
{
    public int TotalDevices { get; init; }
    public int CompliantDevices { get; init; }
    public int NonCompliantDevices { get; init; }
    public int ApproachingEosDevices { get; init; }
    public int UnknownDevices { get; init; }
    public double CompliancePercentage { get; init; }
    public List<OsDistributionDto> OsDistribution { get; init; } = new();
    public List<ComplianceIssueCountDto> TopComplianceIssues { get; init; } = new();
}

public record OsDistributionDto(string OsName, int Count, double Percentage);

public record ComplianceIssueCountDto(string RuleName, int DeviceCount);

public class GetComplianceSummaryQueryHandler : IRequestHandler<GetComplianceSummaryQuery, ComplianceSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetComplianceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceSummaryDto> Handle(
        GetComplianceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await _context.Devices
            .Include(d => d.ComplianceIssues)
            .ToListAsync(cancellationToken);

        var totalDevices = devices.Count;
        var compliantDevices = devices.Count(d => d.ComplianceState == ComplianceState.Compliant);
        var nonCompliantDevices = devices.Count(d => d.ComplianceState == ComplianceState.NonCompliant);
        var approachingEosDevices = devices.Count(d => d.ComplianceState == ComplianceState.ApproachingEndOfSupport);
        var unknownDevices = devices.Count(d => d.ComplianceState == ComplianceState.Unknown);

        var compliancePercentage = totalDevices > 0
            ? Math.Round((double)compliantDevices / totalDevices * 100, 1)
            : 0;

        var osDistribution = devices
            .GroupBy(d => d.OperatingSystem.Name)
            .Select(g => new OsDistributionDto(
                g.Key,
                g.Count(),
                Math.Round((double)g.Count() / totalDevices * 100, 1)))
            .OrderByDescending(o => o.Count)
            .ToList();

        var topComplianceIssues = devices
            .SelectMany(d => d.ComplianceIssues)
            .GroupBy(i => i.RuleName)
            .Select(g => new ComplianceIssueCountDto(g.Key, g.Select(i => i.DeviceId).Distinct().Count()))
            .OrderByDescending(i => i.DeviceCount)
            .Take(5)
            .ToList();

        return new ComplianceSummaryDto
        {
            TotalDevices = totalDevices,
            CompliantDevices = compliantDevices,
            NonCompliantDevices = nonCompliantDevices,
            ApproachingEosDevices = approachingEosDevices,
            UnknownDevices = unknownDevices,
            CompliancePercentage = compliancePercentage,
            OsDistribution = osDistribution,
            TopComplianceIssues = topComplianceIssues
        };
    }
}
