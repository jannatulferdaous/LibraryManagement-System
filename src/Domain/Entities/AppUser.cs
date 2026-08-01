using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class AppUser : BaseAuditableEntity, IAggregateRoot
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AppUser() { } // EF Core

    public static AppUser Create(string fullName, string email, string passwordHash, UserRole role)
        => new()
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
