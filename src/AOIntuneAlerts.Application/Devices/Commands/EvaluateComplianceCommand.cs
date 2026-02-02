using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.Application.Devices.Commands;

public record EvaluateComplianceCommand(Guid? DeviceId = null) : IRequest<Result>;

public class EvaluateComplianceCommandHandler : IRequestHandler<EvaluateComplianceCommand, Result>
{
    private readonly IComplianceEvaluator _complianceEvaluator;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EvaluateComplianceCommandHandler> _logger;

    public EvaluateComplianceCommandHandler(
        IComplianceEvaluator complianceEvaluator,
        IApplicationDbContext context,
        ILogger<EvaluateComplianceCommandHandler> logger)
    {
        _complianceEvaluator = complianceEvaluator;
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(EvaluateComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.DeviceId.HasValue)
            {
                var device = await _context.Devices.FindAsync(new object[] { request.DeviceId.Value }, cancellationToken);
                if (device is null)
                {
                    return Result.Failure($"Device not found: {request.DeviceId}");
                }

                await _complianceEvaluator.EvaluateDeviceComplianceAsync(device, cancellationToken);
                _logger.LogInformation("Compliance evaluation completed for device {DeviceId}", request.DeviceId);
            }
            else
            {
                await _complianceEvaluator.EvaluateAllDevicesAsync(cancellationToken);
                _logger.LogInformation("Compliance evaluation completed for all devices");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating compliance");
            return Result.Failure($"Failed to evaluate compliance: {ex.Message}");
        }
    }
}
