using Application.Common.Specifications;
using Domain.Entities;
using Domain.Enums;

namespace Application.Reservations.Specifications;

/// <summary>
/// Pending reservations for a given book, oldest first - this ordering IS the
/// FIFO reservation queue. Whoever is first in this list is next in line
/// when a copy becomes available (see BookCopy.Return domain event handling, Day 11).
/// </summary>
public class ActiveReservationsForBookSpecification : BaseSpecification<Reservation>
{
    public ActiveReservationsForBookSpecification(Guid bookId)
    {
        AddCriteria(r => r.BookId == bookId && r.Status == ReservationStatus.Pending);
        ApplyOrderBy(r => r.CreatedAt);
    }
}
