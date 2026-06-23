using BookRatingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRatingSystem.Infrastructure.Persistence.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");

        builder.HasKey(book => book.Id);

        builder.Property(book => book.Id)
            .ValueGeneratedNever();

        builder.Property(book => book.Title)
            .HasMaxLength(Book.MaxTitleLength)
            .IsRequired();

        builder.Property(book => book.Author)
            .HasMaxLength(Book.MaxAuthorLength)
            .IsRequired();

        builder.Property(book => book.Category)
            .HasMaxLength(Book.MaxCategoryLength);

        builder.Property(book => book.Description)
            .HasMaxLength(Book.MaxDescriptionLength);

        builder.Property(book => book.CoverImageUrl)
            .HasMaxLength(Book.MaxCoverImageUrlLength);

        builder.Property(book => book.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(book => book.CreatedAt)
            .IsRequired();

        builder.Property(book => book.UpdatedAt)
            .IsRequired();

        builder.HasMany(book => book.Ratings)
            .WithOne(rating => rating.Book)
            .HasForeignKey(rating => rating.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(book => book.Title);
        builder.HasIndex(book => book.Author);
        builder.HasIndex(book => book.Category);

        var createdAt = new DateTimeOffset(2026, 6, 22, 0, 0, 0, TimeSpan.Zero);

        builder.HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Algoritmlar asoslari",
                Author = "A. Karimov",
                Category = "Dasturlash",
                Description = "Algoritm tushunchasi, saralash va qidiruv usullari haqida oʻquv material.",
                PublishedYear = 2024,
                CoverImageUrl = (string?)null,
                Status = BookStatus.Verified,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Maʼlumotlar bazasi",
                Author = "D. Rahimov",
                Category = "Database",
                Description = "Relatsion maʼlumotlar bazasi, SQL va loyihalash asoslari.",
                PublishedYear = 2023,
                CoverImageUrl = (string?)null,
                Status = BookStatus.Verified,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Kompyuter tarmoqlari",
                Author = "S. Aliyev",
                Category = "Tarmoq",
                Description = "Kompyuter tarmoqlarining asosiy protokollari va amaliy qoʻllanilishi.",
                PublishedYear = 2022,
                CoverImageUrl = (string?)null,
                Status = BookStatus.Verified,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
    }
}
