using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Members.Dtos;
using Application.Members.Mappings;
using Application.Members.Specifications;
using Domain.Entities;
using MediatR;

namespace Application.Members.Queries.GetMembers;

public record GetMembersQuery(string? SearchTerm, bool? IsActive, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MemberDto>>;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, PagedResult<MemberDto>>
{
    private readonly IRepository<Member> _memberRepository;

    public GetMembersQueryHandler(IRepository<Member> memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<PagedResult<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var spec = new MembersBySearchSpecification(request.SearchTerm, request.IsActive, request.Page, request.PageSize);

        var members = await _memberRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _memberRepository.CountAsync(spec, cancellationToken);

        return new PagedResult<MemberDto>(members.Select(m => m.ToDto()).ToList(), request.Page, request.PageSize, totalCount);
    }
}
