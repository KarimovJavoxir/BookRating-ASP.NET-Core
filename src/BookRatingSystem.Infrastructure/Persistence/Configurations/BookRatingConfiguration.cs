using BookRatingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRatingSystem.Infrastructure.Persistence.Configurations;

internal sealed class BookRatingConfiguration : IEntityTypeConfiguration<BookRating>
{
    public void Configure(EntityTypeBuilder<BookRating> builder)
    {
        builder.ToTable("book_ratings");

        builder.HasKey(rating => rating.Id);

        builder.Property(rating => rating.Id)
            .ValueGeneratedNever();

        builder.Property(rating => rating.Value)
            .IsRequired();

        builder.Property(rating => rating.Comment)
            .HasMaxLength(BookRating.MaxCommentLength);

        builder.Property(rating => rating.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasDefaultValue(BookRatingStatus.New)
            .IsRequired();

        builder.Property(rating => rating.BanReason)
            .HasColumnName("ban_reason")
            .HasMaxLength(BookRating.MaxBanReasonLength);

        builder.Property(rating => rating.CreatedAt)
            .IsRequired();

        builder.HasIndex(rating => rating.BookId);
        builder.HasIndex(rating => rating.UserId);

        builder.HasOne(rating => rating.User)
            .WithMany(user => user.Ratings)
            .HasForeignKey(rating => rating.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        var createdAt = new DateTimeOffset(2026, 6, 22, 0, 0, 0, TimeSpan.Zero);

        builder.HasData(
            new
            {
                Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                BookId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Value = 5,
                Comment = "Mavzular sodda tushuntirilgan.",
                Status = BookRatingStatus.New,
                BanReason = (string?)null,
                CreatedAt = createdAt.AddMinutes(10)
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222"),
                BookId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Value = 4,
                Comment = "Amaliy misollar foydali.",
                Status = BookRatingStatus.New,
                BanReason = (string?)null,
                CreatedAt = createdAt.AddMinutes(20)
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-3333-3333-3333-333333333333"),
                BookId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                UserId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Value = 5,
                Comment = "Database loyihalash uchun qulay qoʻllanma.",
                Status = BookRatingStatus.New,
                BanReason = (string?)null,
                CreatedAt = createdAt.AddMinutes(30)
            });
    }
}
