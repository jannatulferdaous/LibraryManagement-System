using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Borrowing.Specifications;

public class OverdueLoansSpecification : BaseSpecification<Loan>
{
    public OverdueLoansSpecification(DateTime asOf)
    {
        AddCriteria(loan => loan.ReturnedAt == null && loan.DueDate < asOf);
        ApplyOrderBy(loan => loan.DueDate);
    }
}

/// <summary>
/// All currently open (not yet returned) loans for a specific member -
/// used by Member.CanBorrow-adjacent checks and the member detail screen.
/// </summary>
public class ActiveLoansForMemberSpecification : BaseSpecification<Loan>
{
    public ActiveLoansForMemberSpecification(Guid memberId)
    {
        AddCriteria(loan => loan.MemberId == memberId && loan.ReturnedAt == null);
    }
}
