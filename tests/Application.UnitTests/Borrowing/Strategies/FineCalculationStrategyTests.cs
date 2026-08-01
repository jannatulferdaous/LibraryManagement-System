using Application.Borrowing.Strategies;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Borrowing.Strategies;

public class FineCalculationStrategyTests
{
    private static readonly DateTime DueDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0, 0)]     // returned exactly on time
    [InlineData(-2, 0)]    // returned early
    [InlineData(1, 5)]
    [InlineData(4, 20)]
    public void StandardFineStrategy_CalculatesFiveUnitsPerOverdueDay(int daysLate, decimal expectedFine)
    {
        var strategy = new StandardFineStrategy();
        var returnDate = DueDate.AddDays(daysLate);

        strategy.CalculateFine(DueDate, returnDate).Should().Be(expectedFine);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    [InlineData(50, 50)]   // capped at 50 regardless of how late
    [InlineData(100, 50)]
    public void StudentFineStrategy_CalculatesDiscountedRateWithCap(int daysLate, decimal expectedFine)
    {
        var strategy = new StudentFineStrategy();
        var returnDate = DueDate.AddDays(daysLate);

        strategy.CalculateFine(DueDate, returnDate).Should().Be(expectedFine);
    }

    [Theory]
    [InlineData(1, 0)]     // within the 3-day grace period
    [InlineData(3, 0)]     // exactly at the grace boundary
    [InlineData(4, 3)]     // one day past grace
    [InlineData(6, 9)]
    public void PremiumFineStrategy_AppliesThreeDayGracePeriod(int daysLate, decimal expectedFine)
    {
        var strategy = new PremiumFineStrategy();
        var returnDate = DueDate.AddDays(daysLate);

        strategy.CalculateFine(DueDate, returnDate).Should().Be(expectedFine);
    }

    [Fact]
    public void FineStrategyFactory_ResolvesCorrectStrategyPerMembershipType()
    {
        var factory = new FineStrategyFactory();

        factory.GetStrategy(MembershipType.Student).Should().BeOfType<StudentFineStrategy>();
        factory.GetStrategy(MembershipType.Premium).Should().BeOfType<PremiumFineStrategy>();
        factory.GetStrategy(MembershipType.Standard).Should().BeOfType<StandardFineStrategy>();
    }
}
