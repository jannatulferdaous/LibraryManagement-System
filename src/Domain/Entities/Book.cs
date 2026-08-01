using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Book : BaseAuditableEntity, IAggregateRoot
{
    private readonly List<BookCopy> _copies = new();

    public string Title { get; private set; } = default!;
    public string Author { get; private set; } = default!;
    public string Isbn { get; private set; } = default!;
    public IReadOnlyCollection<BookCopy> Copies => _copies.AsReadOnly();

    private Book() { } // EF Core

    public static Book Create(string title, string author, string isbn)
        => new() { Title = title, Author = author, Isbn = isbn };

    public void UpdateDetails(string title, string author, string isbn)
    {
        Title = title;
        Author = author;
        Isbn = isbn;
    }

    public BookCopy AddCopy(Guid branchId)
    {
        var copy = BookCopy.Create(Id, branchId);
        _copies.Add(copy);
        return copy;
    }

    public void RemoveCopy(Guid copyId)
    {
        var copy = _copies.FirstOrDefault(c => c.Id == copyId)
            ?? throw new NotFoundException(nameof(BookCopy), copyId);

        if (copy.Status == CopyStatus.Borrowed)
            throw new BusinessRuleException("Cannot remove a copy that is currently borrowed.");

        _copies.Remove(copy);
    }
}
