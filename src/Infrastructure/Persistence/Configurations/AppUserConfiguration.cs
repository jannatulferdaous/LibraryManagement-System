using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    // Known GUID and hash so the seed is deterministic across every `dotnet ef database update`.
    private static readonly Guid SeedAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.ConfigureAuditFields();

        // Seeded default Admin account for first login into a fresh database.
        // Email: admin@library.local   Password: Admin@123
        // CHANGE THIS PASSWORD IMMEDIATELY after first login in any real deployment -
        // it exists purely so a reviewer can log in without a manual seeding step.
        builder.HasData(new
        {
            Id = SeedAdminId,
            FullName = "System Administrator",
            Email = "admin@library.local",
            PasswordHash = "$2b$11$WbMMYvIbfRtFRgdT3O1xD.s0nUpJ1dgO4QSXgsFb2jXzCyyE4JWI2",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "seed"
        });
    }
}
