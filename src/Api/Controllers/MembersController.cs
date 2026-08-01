using Application.Common.Models;
using Application.Members.Commands.CreateMember;
using Application.Members.Commands.DeleteMember;
using Application.Members.Commands.UpdateMember;
using Application.Members.Dtos;
using Application.Members.Queries.GetMemberById;
using Application.Members.Queries.GetMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Asp.Versioning;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "CanManageMembers")] // member directory is staff-only, unlike the book catalog
public class MembersController : ControllerBase
{
    private readonly ISender _mediator;

    public MembersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MemberDto>>> GetAll(
        [FromQuery] GetMembersQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemberDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMemberByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateMemberCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route id and body id must match.");
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMemberCommand(id), cancellationToken);
        return NoContent();
    }
}
