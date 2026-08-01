using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public static class AuditableEntityConfigurationExtensions
{
    public static void ConfigureAuditFields<T>(this EntityTypeBuilder<T> builder)
        where T : BaseAuditableEntity
    {
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.ModifiedBy).HasMaxLength(100);
    }
}
