using Application.Common.Interfaces;
using Application.Common.Specifications;
using Application.Reports.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Reports.Queries.GetMostBorrowedBooksReport;

public record GetMostBorrowedBooksReportQuery(int Top = 10) : IRequest<IReadOnlyList<MostBorrowedBookReportDto>>;

public class GetMostBorrowedBooksReportQueryHandler
    : IRequestHandler<GetMostBorrowedBooksReportQuery, IReadOnlyList<MostBorrowedBookReportDto>>
{
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<BookCopy> _bookCopyRepository;
    private readonly IRepository<Book> _bookRepository;

    public GetMostBorrowedBooksReportQueryHandler(
        IRepository<Loan> loanRepository,
        IRepository<BookCopy> bookCopyRepository,
        IRepository<Book> bookRepository)
    {
        _loanRepository = loanRepository;
        _bookCopyRepository = bookCopyRepository;
        _bookRepository = bookRepository;
    }

    public async Task<IReadOnlyList<MostBorrowedBookReportDto>> Handle(
        GetMostBorrowedBooksReportQuery request, CancellationToken cancellationToken)
    {
        // Note: fetches all loans to aggregate. Fine at this data scale for a report; at
        // real production volume this would move to a dedicated read-model or a raw
        // aggregate SQL query instead of loading every Loan row into memory to group it.
        var allLoans = await _loanRepository.ListAsync(cancellationToken);
        if (allLoans.Count == 0)
            return Array.Empty<MostBorrowedBookReportDto>();

        var bookCopyIds = allLoans.Select(l => l.BookCopyId).Distinct();
        var bookCopies = await _bookCopyRepository.ListAsync(new ByIdsSpecification<BookCopy>(bookCopyIds), cancellationToken);
        var copyToBookId = bookCopies.ToDictionary(c => c.Id, c => c.BookId);

        var borrowCountsByBookId = allLoans
            .Where(l => copyToBookId.ContainsKey(l.BookCopyId))
            .GroupBy(l => copyToBookId[l.BookCopyId])
            .ToDictionary(g => g.Key, g => g.Count());

        var topBookIds = borrowCountsByBookId
            .OrderByDescending(kvp => kvp.Value)
            .Take(request.Top)
            .Select(kvp => kvp.Key);

        var books = await _bookRepository.ListAsync(new ByIdsSpecification<Book>(topBookIds), cancellationToken);

        return books
            .Select(b => new MostBorrowedBookReportDto(b.Title, b.Author, borrowCountsByBookId[b.Id]))
            .OrderByDescending(dto => dto.TimesBorrowed)
            .ToList();
    }
}
