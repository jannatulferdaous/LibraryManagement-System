using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Domain;

public class MemberTests
{
    [Fact]
    public void CanBorrow_NewActiveMember_ReturnsTrue()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);

        member.CanBorrow().Should().BeTrue();
    }

    [Fact]
    public void CanBorrow_InactiveMember_ReturnsFalse()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        member.Deactivate();

        member.CanBorrow().Should().BeFalse();
    }

    [Fact]
    public void CanBorrow_AtMaxActiveLoans_ReturnsFalse()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        for (var i = 0; i < 5; i++)
            member.IncrementActiveLoans();

        member.CanBorrow().Should().BeFalse();
    }

    [Fact]
    public void CanBorrow_UnderMaxActiveLoans_ReturnsTrue()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        for (var i = 0; i < 4; i++)
            member.IncrementActiveLoans();

        member.CanBorrow().Should().BeTrue();
    }

    [Fact]
    public void CanBorrow_FinesExceedThreshold_ReturnsFalse()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        member.AddFine(25m); // threshold is 20

        member.CanBorrow().Should().BeFalse();
    }

    [Fact]
    public void CanBorrow_FinesAtExactThreshold_ReturnsTrue()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);
        member.AddFine(20m); // inclusive boundary - CanBorrow uses <=

        member.CanBorrow().Should().BeTrue();
    }

    [Fact]
    public void DecrementActiveLoans_NeverGoesBelowZero()
    {
        var member = Member.Create("Jane Doe", "jane@example.com", MembershipType.Standard);

        member.DecrementActiveLoans();

        member.ActiveLoanCount.Should().Be(0);
    }
}
