using AOIntuneAlerts.Application.Common.Models;
using AOIntuneAlerts.Application.Devices.Commands;
using AOIntuneAlerts.Application.Devices.Queries;
using AOIntuneAlerts.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AOIntuneAlerts.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Viewer")]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<DeviceDto>>> GetDevices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] ComplianceState? complianceState = null,
        [FromQuery] OperatingSystemType? osType = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetDevicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            ComplianceState = complianceState,
            OperatingSystemType = osType,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDetailDto>> GetDevice(Guid id)
    {
        var result = await _mediator.Send(new GetDeviceByIdQuery(id));

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("compliance-summary")]
    [ProducesResponseType(typeof(ComplianceSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComplianceSummaryDto>> GetComplianceSummary()
    {
        var result = await _mediator.Send(new GetComplianceSummaryQuery());
        return Ok(result);
    }

    [HttpPost("sync")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(SyncDevicesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SyncDevicesResult>> SyncDevices()
    {
        var result = await _mediator.Send(new SyncDevicesCommand());

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/evaluate")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EvaluateCompliance(Guid id)
    {
        var result = await _mediator.Send(new EvaluateComplianceCommand(id));

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok();
    }
}
