using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid Id, string Name, string Address, string Phone) : IRequest;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand>
{
    private readonly IRepository<Branch> _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchCommandHandler(IRepository<Branch> branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.Id);

        branch.UpdateDetails(request.Name, request.Address, request.Phone);

        _branchRepository.Update(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
