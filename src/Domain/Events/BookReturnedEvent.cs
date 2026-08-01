using Domain.Common;

namespace Domain.Events;

public class BookReturnedEvent : IDomainEvent
{
    public Guid BookId { get; }
    public Guid BranchId { get; }
    public Guid BookCopyId { get; }
    public DateTime OccurredOn { get; }

    public BookReturnedEvent(Guid bookId, Guid branchId, Guid bookCopyId)
    {
        BookId = bookId;
        BranchId = branchId;
        BookCopyId = bookCopyId;
        OccurredOn = DateTime.UtcNow;
    }
}
