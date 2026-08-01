using Application.Borrowing.Specifications;
using Application.Common.Interfaces;
using Application.Common.Specifications;
using Application.Reports.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Reports.Queries.GetOverdueLoansReport;

public record GetOverdueLoansReportQuery : IRequest<IReadOnlyList<OverdueLoanReportDto>>;

public class GetOverdueLoansReportQueryHandler
    : IRequestHandler<GetOverdueLoansReportQuery, IReadOnlyList<OverdueLoanReportDto>>
{
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<BookCopy> _bookCopyRepository;
    private readonly IRepository<Book> _bookRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOverdueLoansReportQueryHandler(
        IRepository<Loan> loanRepository,
        IRepository<Member> memberRepository,
        IRepository<BookCopy> bookCopyRepository,
        IRepository<Book> bookRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _loanRepository = loanRepository;
        _memberRepository = memberRepository;
        _bookCopyRepository = bookCopyRepository;
        _bookRepository = bookRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<OverdueLoanReportDto>> Handle(
        GetOverdueLoansReportQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        var overdueLoans = await _loanRepository.ListAsync(new OverdueLoansSpecification(now), cancellationToken);
        if (overdueLoans.Count == 0)
            return Array.Empty<OverdueLoanReportDto>();

        // Batch-fetch related entities in 3 queries total (not N+1 per loan), then join
        // in memory - necessary because Loan intentionally has no navigation properties.
        var memberIds = overdueLoans.Select(l => l.MemberId).Distinct();
        var members = await _memberRepository.ListAsync(new ByIdsSpecification<Member>(memberIds), cancellationToken);
        var memberLookup = members.ToDictionary(m => m.Id);

        var bookCopyIds = overdueLoans.Select(l => l.BookCopyId).Distinct();
        var bookCopies = await _bookCopyRepository.ListAsync(new ByIdsSpecification<BookCopy>(bookCopyIds), cancellationToken);
        var bookCopyLookup = bookCopies.ToDictionary(c => c.Id);

        var bookIds = bookCopies.Select(c => c.BookId).Distinct();
        var books = await _bookRepository.ListAsync(new ByIdsSpecification<Book>(bookIds), cancellationToken);
        var bookLookup = books.ToDictionary(b => b.Id);

        return overdueLoans.Select(loan =>
        {
            var member = memberLookup.GetValueOrDefault(loan.MemberId);
            var bookTitle = bookCopyLookup.TryGetValue(loan.BookCopyId, out var copy) && bookLookup.TryGetValue(copy.BookId, out var book)
                ? book.Title
                : "(unknown book)";

            return new OverdueLoanReportDto(
                loan.Id,
                member?.FullName ?? "(unknown member)",
                member?.Email ?? "",
                bookTitle,
                loan.DueDate,
                DaysOverdue: (now.Date - loan.DueDate.Date).Days);
        }).ToList();
    }
}
