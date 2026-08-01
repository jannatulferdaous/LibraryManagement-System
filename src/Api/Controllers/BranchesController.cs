using Application.Branches.Commands.CreateBranch;
using Application.Branches.Commands.DeleteBranch;
using Application.Branches.Commands.UpdateBranch;
using Application.Branches.Dtos;
using Application.Branches.Queries.GetBranchById;
using Application.Branches.Queries.GetBranches;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Asp.Versioning;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly ISender _mediator;

    public BranchesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BranchDto>>> GetAll(
        [FromQuery] GetBranchesQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetBranchByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "CanManageBranches")]
    public async Task<ActionResult<Guid>> Create(CreateBranchCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageBranches")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateBranchCommand(id, request.Name, request.Address, request.Phone), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManageBranches")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBranchCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateBranchRequest(string Name, string Address, string Phone);
