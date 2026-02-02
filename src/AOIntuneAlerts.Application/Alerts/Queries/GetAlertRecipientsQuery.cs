using AOIntuneAlerts.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Alerts.Queries;

public record GetAlertRecipientsQuery : IRequest<List<AlertRecipientDto>>;

public record AlertRecipientDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string MinimumSeverity { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class GetAlertRecipientsQueryHandler : IRequestHandler<GetAlertRecipientsQuery, List<AlertRecipientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertRecipientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlertRecipientDto>> Handle(
        GetAlertRecipientsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.AlertRecipients
            .OrderBy(r => r.DisplayName)
            .Select(r => new AlertRecipientDto
            {
                Id = r.Id,
                Email = r.Email.Value,
                DisplayName = r.DisplayName,
                IsEnabled = r.IsEnabled,
                MinimumSeverity = r.MinimumSeverity.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
