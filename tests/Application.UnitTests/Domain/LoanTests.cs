using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Domain;

public class LoanTests
{
    [Fact]
    public void Create_SetsDueDateFourteenDaysOut_ByDefault()
    {
        var borrowedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), borrowedAt);

        loan.DueDate.Should().Be(borrowedAt.AddDays(14));
    }

    [Fact]
    public void IsOverdue_PastDueDateAndNotReturned_ReturnsTrue()
    {
        var borrowedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), borrowedAt, loanPeriodDays: 14);

        var isOverdue = loan.IsOverdue(borrowedAt.AddDays(20));

        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_BeforeDueDate_ReturnsFalse()
    {
        var borrowedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), borrowedAt, loanPeriodDays: 14);

        var isOverdue = loan.IsOverdue(borrowedAt.AddDays(5));

        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_AfterReturned_ReturnsFalseEvenIfPastDueDate()
    {
        var borrowedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), borrowedAt, loanPeriodDays: 14);
        loan.MarkReturned(borrowedAt.AddDays(20), fine: 30m);

        var isOverdue = loan.IsOverdue(borrowedAt.AddDays(25));

        isOverdue.Should().BeFalse();
    }
}
