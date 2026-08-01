using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(256);
        builder.Property(m => m.MembershipType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.OutstandingFines).HasColumnType("decimal(10,2)");

        builder.HasIndex(m => m.Email).IsUnique();

        builder.ConfigureAuditFields();
    }
}
