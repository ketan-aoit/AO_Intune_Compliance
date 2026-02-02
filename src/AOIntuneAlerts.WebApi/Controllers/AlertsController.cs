using AOIntuneAlerts.Application.Alerts.Commands;
using AOIntuneAlerts.Application.Alerts.Queries;
using AOIntuneAlerts.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AOIntuneAlerts.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Viewer")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("recipients")]
    [ProducesResponseType(typeof(List<AlertRecipientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertRecipientDto>>> GetRecipients()
    {
        var result = await _mediator.Send(new GetAlertRecipientsQuery());
        return Ok(result);
    }

    [HttpPost("recipients")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfigureRecipients(
        [FromBody] ConfigureAlertRecipientsCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(PaginatedList<AlertHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<AlertHistoryDto>>> GetHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? deviceId = null)
    {
        var query = new GetAlertHistoryQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            FromDate = fromDate,
            ToDate = toDate,
            DeviceId = deviceId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("send")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendAlert([FromBody] SendAlertCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}
