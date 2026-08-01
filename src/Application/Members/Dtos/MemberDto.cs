using Domain.Enums;

namespace Application.Members.Dtos;

public class MemberDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public MembershipType MembershipType { get; init; }
    public bool IsActive { get; init; }
    public decimal OutstandingFines { get; init; }
    public int ActiveLoanCount { get; init; }
}
