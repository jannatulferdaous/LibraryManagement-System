using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(Guid ReservationId) : IRequest;

public class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
    }
}

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationCommandHandler(IRepository<Reservation> reservationRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.Status != ReservationStatus.Pending)
            throw new BusinessRuleException("Only pending reservations can be cancelled.");

        reservation.Cancel();
        _reservationRepository.Update(reservation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
