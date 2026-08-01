using System.Linq.Expressions;
using Application.Common.Interfaces;

namespace Application.Common.Specifications;

/// <summary>
/// Wraps an existing specification but exposes only its Criteria - used by
/// IRepository.CountAsync so a total row count isn't skewed by Skip/Take,
/// and doesn't waste time joining Includes it doesn't need.
/// </summary>
public class CountOnlySpecification<T> : BaseSpecification<T>
{
    public CountOnlySpecification(ISpecification<T> source)
    {
        if (source.Criteria is not null)
            AddCriteria(source.Criteria);
    }
}
