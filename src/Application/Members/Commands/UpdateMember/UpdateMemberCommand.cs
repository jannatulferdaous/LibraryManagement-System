using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;

namespace Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand(Guid Id, string FullName, string Email, MembershipType MembershipType) : IRequest;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(IRepository<Member> memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        member.UpdateDetails(request.FullName, request.Email, request.MembershipType);

        _memberRepository.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
