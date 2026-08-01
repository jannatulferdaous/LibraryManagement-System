using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Branches.Specifications;

public class BranchesBySearchSpecification : BaseSpecification<Branch>
{
    public BranchesBySearchSpecification(string? searchTerm, int page, int pageSize)
    {
        AddCriteria(branch =>
            string.IsNullOrWhiteSpace(searchTerm)
            || branch.Name.Contains(searchTerm)
            || branch.Address.Contains(searchTerm));

        ApplyOrderBy(b => b.Name);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
