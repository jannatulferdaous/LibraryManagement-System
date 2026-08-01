using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Member : BaseAuditableEntity, IAggregateRoot
{
    private const int MaxActiveLoans = 5;
    private const decimal MaxOutstandingFinesToBorrow = 20m;

    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public MembershipType MembershipType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public decimal OutstandingFines { get; private set; }
    public int ActiveLoanCount { get; private set; }

    private Member() { } // EF Core

    public static Member Create(string fullName, string email, MembershipType type)
        => new() { FullName = fullName, Email = email, MembershipType = type };

    public void UpdateDetails(string fullName, string email, MembershipType membershipType)
    {
        FullName = fullName;
        Email = email;
        MembershipType = membershipType;
    }

    public bool CanBorrow()
        => IsActive && ActiveLoanCount < MaxActiveLoans && OutstandingFines <= MaxOutstandingFinesToBorrow;

    public void IncrementActiveLoans() => ActiveLoanCount++;

    public void DecrementActiveLoans() => ActiveLoanCount = Math.Max(0, ActiveLoanCount - 1);

    public void AddFine(decimal amount) => OutstandingFines += amount;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
