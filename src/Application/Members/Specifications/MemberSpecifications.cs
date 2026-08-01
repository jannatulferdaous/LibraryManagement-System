using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Members.Specifications;

public class MembersBySearchSpecification : BaseSpecification<Member>
{
    public MembersBySearchSpecification(string? searchTerm, bool? isActive, int page, int pageSize)
    {
        AddCriteria(member =>
            (string.IsNullOrWhiteSpace(searchTerm)
                || member.FullName.Contains(searchTerm)
                || member.Email.Contains(searchTerm))
            && (isActive == null || member.IsActive == isActive));

        ApplyOrderBy(m => m.FullName);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
