using Application.Borrowing.Strategies;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Borrowing.Commands.ReturnBook;

public record ReturnBookCommand(Guid LoanId) : IRequest;

public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty();
    }
}

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand>
{
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<BookCopy> _bookCopyRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFineStrategyFactory _fineStrategyFactory;
    private readonly IDistributedCache _cache;

    public ReturnBookCommandHandler(
        IRepository<Loan> loanRepository,
        IRepository<BookCopy> bookCopyRepository,
        IRepository<Member> memberRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IFineStrategyFactory fineStrategyFactory,
        IDistributedCache cache)
    {
        _loanRepository = loanRepository;
        _bookCopyRepository = bookCopyRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _fineStrategyFactory = fineStrategyFactory;
        _cache = cache;
    }

    public async Task Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Loan), request.LoanId);

        if (loan.ReturnedAt is not null)
            throw new BusinessRuleException("This loan has already been returned.");

        var bookCopy = await _bookCopyRepository.GetByIdAsync(loan.BookCopyId, cancellationToken)
            ?? throw new NotFoundException(nameof(BookCopy), loan.BookCopyId);

        var member = await _memberRepository.GetByIdAsync(loan.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), loan.MemberId);

        var now = _dateTimeProvider.UtcNow;

        // Strategy pattern: fine calculation varies by membership type. The handler
        // doesn't know or care HOW the fine is computed, only that it asks the factory
        // for the right strategy and uses whatever it returns.
        var fineStrategy = _fineStrategyFactory.GetStrategy(member.MembershipType);
        var fine = fineStrategy.CalculateFine(loan.DueDate, now);

        loan.MarkReturned(now, fine);
        _loanRepository.Update(loan);

        // Raises BookReturnedEvent internally - ReservationFulfillmentHandler (below)
        // reacts to it independently; this handler has no idea that handler even exists.
        bookCopy.Return();
        _bookCopyRepository.Update(bookCopy);

        member.DecrementActiveLoans();
        if (fine > 0)
            member.AddFine(fine);
        _memberRepository.Update(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // AvailableCopies changed - same reasoning as BorrowBookCommandHandler.
        await _cache.RemoveAsync($"book:{bookCopy.BookId}", cancellationToken);
    }
}
