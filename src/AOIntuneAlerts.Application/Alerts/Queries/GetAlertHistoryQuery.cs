using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Alerts.Queries;

public record GetAlertHistoryQuery : IRequest<PaginatedList<AlertHistoryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? DeviceId { get; init; }
}

public record AlertHistoryDto
{
    public Guid Id { get; init; }
    public Guid? DeviceId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
    public bool WasSent { get; init; }
    public string? ErrorMessage { get; init; }
    public int RecipientCount { get; init; }
}

public class GetAlertHistoryQueryHandler : IRequestHandler<GetAlertHistoryQuery, PaginatedList<AlertHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAlertHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AlertHistoryDto>> Handle(
        GetAlertHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Alerts
            .Include(a => a.Recipients)
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= request.ToDate.Value);
        }

        if (request.DeviceId.HasValue)
        {
            query = query.Where(a => a.DeviceId == request.DeviceId.Value);
        }

        var dtoQuery = query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertHistoryDto
            {
                Id = a.Id,
                DeviceId = a.DeviceId,
                Subject = a.Subject,
                Severity = a.Severity.ToString(),
                CreatedAt = a.CreatedAt,
                SentAt = a.SentAt,
                WasSent = a.WasSent,
                ErrorMessage = a.ErrorMessage,
                RecipientCount = a.Recipients.Count
            });

        return await PaginatedList<AlertHistoryDto>.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
