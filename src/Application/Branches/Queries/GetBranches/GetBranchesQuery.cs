using Application.Branches.Dtos;
using Application.Branches.Mappings;
using Application.Branches.Specifications;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Branches.Queries.GetBranches;

public record GetBranchesQuery(string? SearchTerm, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<BranchDto>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, PagedResult<BranchDto>>
{
    private readonly IRepository<Branch> _branchRepository;

    public GetBranchesQueryHandler(IRepository<Branch> branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<PagedResult<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var spec = new BranchesBySearchSpecification(request.SearchTerm, request.Page, request.PageSize);

        var branches = await _branchRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _branchRepository.CountAsync(spec, cancellationToken);

        return new PagedResult<BranchDto>(branches.Select(b => b.ToDto()).ToList(), request.Page, request.PageSize, totalCount);
    }
}
