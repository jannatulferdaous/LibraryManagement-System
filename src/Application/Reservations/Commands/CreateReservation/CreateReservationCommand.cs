using Application.Common.Interfaces;
using Application.Reservations.Specifications;
using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(Guid BookId, Guid MemberId) : IRequest<Guid>;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IRepository<Reservation> _reservationRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(
        IRepository<Reservation> reservationRepository,
        IRepository<Member> memberRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.MemberId);

        if (!member.IsActive)
            throw new BusinessRuleException("Inactive members cannot place reservations.");

        var existingForBook = await _reservationRepository.ListAsync(
            new ActiveReservationsForBookSpecification(request.BookId), cancellationToken);

        if (existingForBook.Any(r => r.MemberId == request.MemberId))
            throw new BusinessRuleException("This member already has a pending reservation for this book.");

        var reservation = Reservation.Create(request.BookId, request.MemberId);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
