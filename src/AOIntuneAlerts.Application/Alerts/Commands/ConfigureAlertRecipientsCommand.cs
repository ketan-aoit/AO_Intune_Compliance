using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Domain.Aggregates.Alert;
using AOIntuneAlerts.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Alerts.Commands;

public record ConfigureAlertRecipientsCommand : IRequest<Result>
{
    public List<AlertRecipientInput> Recipients { get; init; } = new();
}

public record AlertRecipientInput
{
    public Guid? Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public AlertSeverity MinimumSeverity { get; init; }
    public bool IsEnabled { get; init; }
    public bool IsDeleted { get; init; }
}

public class ConfigureAlertRecipientsCommandValidator : AbstractValidator<ConfigureAlertRecipientsCommand>
{
    public ConfigureAlertRecipientsCommandValidator()
    {
        RuleForEach(x => x.Recipients).ChildRules(recipient =>
        {
            recipient.RuleFor(r => r.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            recipient.RuleFor(r => r.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");
        });
    }
}

public class ConfigureAlertRecipientsCommandHandler : IRequestHandler<ConfigureAlertRecipientsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ConfigureAlertRecipientsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        ConfigureAlertRecipientsCommand request,
        CancellationToken cancellationToken)
    {
        var existingRecipients = await _context.AlertRecipients.ToListAsync(cancellationToken);

        foreach (var input in request.Recipients)
        {
            if (input.Id.HasValue)
            {
                var existing = existingRecipients.FirstOrDefault(r => r.Id == input.Id.Value);
                if (existing is not null)
                {
                    if (input.IsDeleted)
                    {
                        _context.AlertRecipients.Remove(existing);
                    }
                    else
                    {
                        existing.Update(input.DisplayName, input.MinimumSeverity);
                        if (input.IsEnabled)
                            existing.Enable();
                        else
                            existing.Disable();
                    }
                }
            }
            else if (!input.IsDeleted)
            {
                var recipient = AlertRecipient.Create(
                    input.Email,
                    input.DisplayName,
                    input.MinimumSeverity);

                if (!input.IsEnabled)
                    recipient.Disable();

                _context.AlertRecipients.Add(recipient);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
