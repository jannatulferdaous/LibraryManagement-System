using Domain.Common;

namespace Domain.Entities;

public class Loan : BaseAuditableEntity, IAggregateRoot
{
    public Guid BookCopyId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime BorrowedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public decimal FineAmount { get; private set; }

    private Loan() { } // EF Core

    public static Loan Create(Guid bookCopyId, Guid memberId, DateTime borrowedAt, int loanPeriodDays = 14)
        => new()
        {
            BookCopyId = bookCopyId,
            MemberId = memberId,
            BorrowedAt = borrowedAt,
            DueDate = borrowedAt.AddDays(loanPeriodDays)
        };

    public bool IsOverdue(DateTime asOf) => ReturnedAt is null && asOf > DueDate;

    public void MarkReturned(DateTime returnedAt, decimal fine)
    {
        ReturnedAt = returnedAt;
        FineAmount = fine;
    }
}
