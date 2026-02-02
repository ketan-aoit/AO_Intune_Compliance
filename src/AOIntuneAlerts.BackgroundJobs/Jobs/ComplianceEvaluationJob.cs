using AOIntuneAlerts.Application.Devices.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AOIntuneAlerts.BackgroundJobs.Jobs;

public class ComplianceEvaluationJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplianceEvaluationJob> _logger;

    public ComplianceEvaluationJob(IMediator mediator, ILogger<ComplianceEvaluationJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting compliance evaluation job");

        try
        {
            var result = await _mediator.Send(new EvaluateComplianceCommand());

            if (result.IsSuccess)
            {
                _logger.LogInformation("Compliance evaluation completed successfully");
            }
            else
            {
                _logger.LogError("Compliance evaluation failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during compliance evaluation job");
            throw;
        }
    }
}
