using Application.Common.Interfaces;
using Application.Members.Dtos;
using Application.Members.Mappings;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(Guid Id) : IRequest<MemberDto>;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto>
{
    private readonly IRepository<Member> _memberRepository;

    public GetMemberByIdQueryHandler(IRepository<Member> memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<MemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), request.Id);

        return member.ToDto();
    }
}
