using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Author).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Isbn).IsRequired().HasMaxLength(20);

        builder.HasIndex(b => b.Isbn).IsUnique();
        builder.HasIndex(b => b.Title);

        // Book.Copies is exposed as IReadOnlyCollection<BookCopy> backed by a private
        // List<BookCopy> field - EF needs to be told to use the field directly, since
        // there's no public setter for the collection to bind to.
        builder.HasMany(b => b.Copies)
            .WithOne()
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.Copies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ConfigureAuditFields();
    }
}
