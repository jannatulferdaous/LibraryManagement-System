using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("BookCopies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // SQL Server rowversion - the actual optimistic concurrency mechanism. IsRowVersion()
        // tells EF this column is the concurrency token; SQL Server auto-updates its value
        // on every UPDATE to this row, no application code increments it manually.
        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasIndex(c => new { c.BranchId, c.Status });

        // FK to Member/Loan intentionally not modeled here - a copy doesn't know who
        // borrowed it; that relationship lives on Loan instead (see LoanConfiguration).
    }
}
