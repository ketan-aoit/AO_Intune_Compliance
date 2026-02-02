using AOIntuneAlerts.Application.ComplianceRules.Commands;
using AOIntuneAlerts.Application.ComplianceRules.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AOIntuneAlerts.WebApi.Controllers;

[ApiController]
[Route("api/v1/compliance-rules")]
[Authorize(Policy = "Viewer")]
public class ComplianceRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplianceRulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ComplianceRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ComplianceRuleDto>>> GetRules(
        [FromQuery] bool includeDisabled = false)
    {
        var result = await _mediator.Send(new GetComplianceRulesQuery(includeDisabled));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateRule([FromBody] CreateComplianceRuleCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetRules), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateComplianceRuleCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route ID does not match command ID");

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Ok();
    }
}
