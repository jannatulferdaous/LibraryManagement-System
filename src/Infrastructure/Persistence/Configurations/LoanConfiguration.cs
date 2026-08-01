using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.FineAmount).HasColumnType("decimal(10,2)");

        // FK constraints without navigation properties on either side - Loan is its own
        // aggregate root and only references Member/BookCopy by Id in code (DDD aggregate
        // boundary), but the database still enforces referential integrity.
        builder.HasOne<Member>().WithMany().HasForeignKey(l => l.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<BookCopy>().WithMany().HasForeignKey(l => l.BookCopyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.MemberId);
        builder.HasIndex(l => l.BookCopyId);
        builder.HasIndex(l => new { l.ReturnedAt, l.DueDate }); // supports OverdueLoansSpecification

        builder.ConfigureAuditFields();
    }
}
