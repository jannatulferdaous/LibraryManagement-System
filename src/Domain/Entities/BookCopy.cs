using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Entities;

public class BookCopy : BaseEntity
{
    public Guid BookId { get; private set; }
    public Guid BranchId { get; private set; }
    public CopyStatus Status { get; private set; } = CopyStatus.Available;

    // Optimistic concurrency token - SQL Server auto-increments this on every UPDATE.
    // EF Core compares it in the WHERE clause of its UPDATE statement; a mismatch means
    // someone else changed this row since it was read, and throws DbUpdateConcurrencyException.
    // Set by EF via the configuration in BookCopyConfiguration - never touched in code.
    public byte[] RowVersion { get; private set; } = default!;

    private BookCopy() { } // EF Core

    public static BookCopy Create(Guid bookId, Guid branchId)
        => new() { BookId = bookId, BranchId = branchId, Status = CopyStatus.Available };

    public void Borrow()
    {
        if (Status != CopyStatus.Available)
            throw new BusinessRuleException($"Copy is not available (current status: {Status}).");

        Status = CopyStatus.Borrowed;
    }

    public void Return()
    {
        if (Status != CopyStatus.Borrowed)
            throw new BusinessRuleException("Copy is not currently borrowed.");

        Status = CopyStatus.Available;

        AddDomainEvent(new BookReturnedEvent(BookId, BranchId, Id));
    }
}
