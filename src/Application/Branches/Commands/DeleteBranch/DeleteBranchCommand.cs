using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand>
{
    private readonly IRepository<Branch> _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBranchCommandHandler(IRepository<Branch> branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.Id);

        // Note: a stricter rule (reject delete if branch still has copies/loans) is a
        // good candidate to add once Reports/Borrowing specs exist to check against.
        _branchRepository.Remove(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
