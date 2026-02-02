using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Aggregates.ComplianceRule;
using AOIntuneAlerts.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AOIntuneAlerts.Application.ComplianceRules.Commands;

public record CreateComplianceRuleCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ComplianceRuleType RuleType { get; init; }
    public AlertSeverity Severity { get; init; }
    public string Configuration { get; init; } = "{}";
    public OperatingSystemType? ApplicableOs { get; init; }
}

public class CreateComplianceRuleCommandValidator : AbstractValidator<CreateComplianceRuleCommand>
{
    public CreateComplianceRuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(200).WithMessage("Rule name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Configuration)
            .NotEmpty().WithMessage("Configuration is required");
    }
}

public class CreateComplianceRuleCommandHandler : IRequestHandler<CreateComplianceRuleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateComplianceRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateComplianceRuleCommand request,
        CancellationToken cancellationToken)
    {
        var rule = ComplianceRule.Create(
            request.Name,
            request.Description,
            request.RuleType,
            request.Severity,
            request.Configuration,
            request.ApplicableOs);

        _context.ComplianceRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.Id);
    }
}
