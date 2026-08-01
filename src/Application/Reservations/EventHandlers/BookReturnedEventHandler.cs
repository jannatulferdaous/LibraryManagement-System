using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Reservations.Specifications;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Reservations.EventHandlers;

public class BookReturnedEventHandler : INotificationHandler<DomainEventNotification<BookReturnedEvent>>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationFactory _notificationFactory;
    private readonly ILogger<BookReturnedEventHandler> _logger;

    public BookReturnedEventHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Member> memberRepository,
        IUnitOfWork unitOfWork,
        INotificationFactory notificationFactory,
        ILogger<BookReturnedEventHandler> logger)
    {
        _reservationRepository = reservationRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _notificationFactory = notificationFactory;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<BookReturnedEvent> notification, CancellationToken cancellationToken)
    {
        var bookId = notification.DomainEvent.BookId;

        var spec = new ActiveReservationsForBookSpecification(bookId);
        var nextInLine = (await _reservationRepository.ListAsync(spec, cancellationToken))
            .OrderBy(r => r.CreatedAt) // FIFO - spec already orders this, re-asserted here for clarity
            .FirstOrDefault();

        if (nextInLine is null)
        {
            _logger.LogInformation("Book {BookId} returned - no pending reservations", bookId);
            return;
        }

        nextInLine.Fulfill();
        _reservationRepository.Update(nextInLine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var member = await _memberRepository.GetByIdAsync(nextInLine.MemberId, cancellationToken);
        if (member is null)
        {
            _logger.LogWarning("Reservation {ReservationId} fulfilled but member {MemberId} not found - skipping notification",
                nextInLine.Id, nextInLine.MemberId);
            return;
        }

        // Factory pattern in action: this handler doesn't know or care which channel
        // actually sends the message, just that it asks for one and calls SendAsync.
        var notifier = _notificationFactory.Create(NotificationChannel.Email);
        await notifier.SendAsync(
            member.Email,
            "Your reserved book is available",
            $"Hi {member.FullName}, a copy of your reserved book is now available for pickup.",
            cancellationToken);

        _logger.LogInformation("Reservation {ReservationId} fulfilled for member {MemberId}, notification sent",
            nextInLine.Id, member.Id);
    }
}
