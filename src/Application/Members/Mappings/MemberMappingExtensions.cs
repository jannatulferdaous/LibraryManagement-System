using Application.Members.Dtos;
using Domain.Entities;

namespace Application.Members.Mappings;

public static class MemberMappingExtensions
{
    public static MemberDto ToDto(this Member member) => new()
    {
        Id = member.Id,
        FullName = member.FullName,
        Email = member.Email,
        MembershipType = member.MembershipType,
        IsActive = member.IsActive,
        OutstandingFines = member.OutstandingFines,
        ActiveLoanCount = member.ActiveLoanCount
    };
}
