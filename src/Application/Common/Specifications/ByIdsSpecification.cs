using Application.Common.Interfaces;
using Domain.Common;

namespace Application.Common.Specifications;

public class ByIdsSpecification<T> : BaseSpecification<T> where T : BaseEntity
{
    public ByIdsSpecification(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        AddCriteria(entity => idSet.Contains(entity.Id));
    }
}
