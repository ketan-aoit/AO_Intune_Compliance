using AOIntuneAlerts.Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AOIntuneAlerts.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Viewer")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var result = await _mediator.Send(new GetDashboardQuery());
        return Ok(result);
    }
}
