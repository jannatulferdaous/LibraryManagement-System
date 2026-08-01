using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Borrowing.Commands.BorrowBook;

public record BorrowBookCommand(Guid BookCopyId, Guid MemberId) : IRequest<Guid>;

public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.BookCopyId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Guid>
{
    private readonly IRepository<BookCopy> _bookCopyRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<Loan> _loanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDistributedCache _cache;

    public BorrowBookCommandHandler(
        IRepository<BookCopy> bookCopyRepository,
        IRepository<Member> memberRepository,
        IRepository<Loan> loanRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IDistributedCache cache)
    {
        _bookCopyRepository = bookCopyRepository;
        _memberRepository = memberRepository;
        _loanRepository = loanRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
    }

    public async Task<Guid> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        if (!member.CanBorrow())
            throw new BusinessRuleException("Member is not eligible to borrow: inactive, at loan limit, or fines exceed the allowed threshold.");

        var bookCopy = await _bookCopyRepository.GetByIdAsync(request.BookCopyId, cancellationToken)
            ?? throw new NotFoundException(nameof(BookCopy), request.BookCopyId);

        // Throws BusinessRuleException if already borrowed/lost/under maintenance.
        // If two requests race for the SAME copy, EF Core's optimistic concurrency check
        // (BookCopy.RowVersion, Day 9) catches the loser at SaveChangesAsync below with a
        // DbUpdateConcurrencyException - mapped to 409 by GlobalExceptionHandler (Day 9).
        bookCopy.Borrow();
        _bookCopyRepository.Update(bookCopy);

        var loan = Loan.Create(bookCopy.Id, member.Id, _dateTimeProvider.UtcNow);
        await _loanRepository.AddAsync(loan, cancellationToken);

        member.IncrementActiveLoans();
        _memberRepository.Update(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // AvailableCopies on the cached GetBookByIdQuery result just changed - invalidate
        // rather than let it serve a stale count for up to 5 minutes (Redis, Day 13 bonus).
        await _cache.RemoveAsync($"book:{bookCopy.BookId}", cancellationToken);

        return loan.Id;
    }
}
