using Application.Borrowing.Commands.BorrowBook;
using Application.Borrowing.Commands.ReturnBook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Asp.Versioning;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "CanManageCatalog")] // Librarian/Admin process borrows and returns at the desk
public class BorrowingController : ControllerBase
{
    private readonly ISender _mediator;

    public BorrowingController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<Guid>> Borrow(BorrowBookCommand command, CancellationToken cancellationToken)
    {
        var loanId = await _mediator.Send(command, cancellationToken);
        return Ok(new { loanId });
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return(ReturnBookCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
