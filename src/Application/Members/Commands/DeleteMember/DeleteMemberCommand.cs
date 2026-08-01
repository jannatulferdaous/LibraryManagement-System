using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMemberCommandHandler(IRepository<Member> memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        if (member.ActiveLoanCount > 0)
            throw new BusinessRuleException("Cannot delete a member with active loans. Return all books first.");

        if (member.OutstandingFines > 0)
            throw new BusinessRuleException("Cannot delete a member with outstanding fines.");

        _memberRepository.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
