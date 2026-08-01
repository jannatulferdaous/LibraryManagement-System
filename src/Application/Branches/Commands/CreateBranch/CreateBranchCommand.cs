using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Branches.Commands.CreateBranch;

public record CreateBranchCommand(string Name, string Address, string Phone) : IRequest<Guid>;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IRepository<Branch> _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(IRepository<Branch> branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = Branch.Create(request.Name, request.Address, request.Phone);

        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return branch.Id;
    }
}
