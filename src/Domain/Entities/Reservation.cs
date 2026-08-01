using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Reservation : BaseAuditableEntity, IAggregateRoot
{
    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;

    private Reservation() { } // EF Core

    public static Reservation Create(Guid bookId, Guid memberId)
        => new() { BookId = bookId, MemberId = memberId };

    public void Fulfill() => Status = ReservationStatus.Fulfilled;

    public void Cancel() => Status = ReservationStatus.Cancelled;
}
