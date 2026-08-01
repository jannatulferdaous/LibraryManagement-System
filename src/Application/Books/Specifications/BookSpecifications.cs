using Application.Common.Specifications;
using Domain.Entities;
using Domain.Enums;

namespace Application.Books.Specifications;

public class BooksBySearchSpecification : BaseSpecification<Book>
{
    public BooksBySearchSpecification(string? searchTerm, Guid? branchId, int page, int pageSize)
    {
        AddCriteria(book =>
            (string.IsNullOrWhiteSpace(searchTerm)
                || book.Title.Contains(searchTerm)
                || book.Author.Contains(searchTerm)
                || book.Isbn.Contains(searchTerm))
            && (branchId == null || book.Copies.Any(c => c.BranchId == branchId)));

        AddInclude(b => b.Copies);
        ApplyOrderBy(b => b.Title);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Narrower specification for the borrow workflow: only books that currently
/// have at least one available copy at the given branch. Kept separate from
/// BooksBySearchSpecification because "browse everything" and "what can I
/// actually lend right now" are different questions with different callers.
/// </summary>
public class AvailableBooksAtBranchSpecification : BaseSpecification<Book>
{
    public AvailableBooksAtBranchSpecification(Guid branchId, string? searchTerm = null)
    {
        AddCriteria(book =>
            book.Copies.Any(c => c.BranchId == branchId && c.Status == CopyStatus.Available)
            && (string.IsNullOrWhiteSpace(searchTerm)
                || book.Title.Contains(searchTerm)
                || book.Author.Contains(searchTerm)));

        AddInclude(b => b.Copies);
    }
}
