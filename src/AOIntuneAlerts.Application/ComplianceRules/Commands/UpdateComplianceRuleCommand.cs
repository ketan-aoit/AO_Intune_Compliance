using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AOIntuneAlerts.Application.ComplianceRules.Commands;

public record UpdateComplianceRuleCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AlertSeverity Severity { get; init; }
    public string Configuration { get; init; } = "{}";
    public OperatingSystemType? ApplicableOs { get; init; }
    public bool IsEnabled { get; init; }
}

public class UpdateComplianceRuleCommandValidator : AbstractValidator<UpdateComplianceRuleCommand>
{
    public UpdateComplianceRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rule ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rule name is required")
            .MaximumLength(200).WithMessage("Rule name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Configuration)
            .NotEmpty().WithMessage("Configuration is required");
    }
}

public class UpdateComplianceRuleCommandHandler : IRequestHandler<UpdateComplianceRuleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateComplianceRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        UpdateComplianceRuleCommand request,
        CancellationToken cancellationToken)
    {
        var rule = await _context.ComplianceRules.FindAsync(
            new object[] { request.Id }, cancellationToken);

        if (rule is null)
        {
            return Result.Failure($"Compliance rule not found: {request.Id}");
        }

        rule.Update(
            request.Name,
            request.Description,
            request.Severity,
            request.Configuration,
            request.ApplicableOs);

        if (request.IsEnabled)
            rule.Enable();
        else
            rule.Disable();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
