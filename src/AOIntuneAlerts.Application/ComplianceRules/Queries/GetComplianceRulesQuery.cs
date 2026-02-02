using AOIntuneAlerts.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.ComplianceRules.Queries;

public record GetComplianceRulesQuery(bool IncludeDisabled = false) : IRequest<List<ComplianceRuleDto>>;

public record ComplianceRuleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string RuleType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Configuration { get; init; } = "{}";
    public string? ApplicableOs { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class GetComplianceRulesQueryHandler : IRequestHandler<GetComplianceRulesQuery, List<ComplianceRuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetComplianceRulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComplianceRuleDto>> Handle(
        GetComplianceRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ComplianceRules.AsQueryable();

        if (!request.IncludeDisabled)
        {
            query = query.Where(r => r.IsEnabled);
        }

        return await query
            .OrderBy(r => r.Name)
            .Select(r => new ComplianceRuleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                RuleType = r.RuleType.ToString(),
                IsEnabled = r.IsEnabled,
                Severity = r.Severity.ToString(),
                Configuration = r.Configuration,
                ApplicableOs = r.ApplicableOs.HasValue ? r.ApplicableOs.Value.ToString() : null,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
