using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Tests;

public sealed class EfBookRatingRepositoryTests
{
    [Fact]
    public async Task ListPendingCommentRatingsAsync_returns_new_ratings_oldest_first()
    {
        var options = new DbContextOptionsBuilder<BookRatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new BookRatingDbContext(options);
        var now = new DateTimeOffset(2026, 6, 25, 11, 0, 0, TimeSpan.Zero);
        var bookId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
        var userId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
        dbContext.Books.Add(Book.Create(
            bookId,
            "Algoritmlar",
            "A. Muallif",
            "Dasturlash",
            null,
            2026,
            null,
            now));
        dbContext.Users.Add(User.Create(
            userId,
            "comment-review-user",
            "comment-review-user@example.com",
            "hash",
            null,
            isAdmin: false,
            now));
        var newerNewRating = BookRating.Create(
            Guid.Parse("cccccccc-cccc-4ccc-cccc-ccccccccccc2"),
            bookId,
            userId,
            value: 4,
            comment: "Ikkinchi izoh",
            now.AddMinutes(2));
        var olderNewRating = BookRating.Create(
            Guid.Parse("cccccccc-cccc-4ccc-cccc-ccccccccccc1"),
            bookId,
            userId,
            value: 5,
            comment: "Birinchi izoh",
            now.AddMinutes(1));
        var verifiedRating = BookRating.Create(
            Guid.Parse("cccccccc-cccc-4ccc-cccc-ccccccccccc3"),
            bookId,
            userId,
            value: 3,
            comment: "Tekshirilgan izoh",
            now.AddMinutes(3),
            BookRatingStatus.Verified);
        dbContext.BookRatings.AddRange(newerNewRating, olderNewRating, verifiedRating);
        await dbContext.SaveChangesAsync();
        var repositoryType = typeof(BookRatingDbContext).Assembly.GetType(
            "BookRatingSystem.Infrastructure.Repositories.EfBookRatingRepository",
            throwOnError: true)!;
        var repository = (IBookRatingRepository)Activator.CreateInstance(repositoryType, dbContext)!;

        var pendingRatings = await repository.ListPendingCommentRatingsAsync(limit: 10, CancellationToken.None);

        Assert.Equal(
            [olderNewRating.Id, newerNewRating.Id],
            pendingRatings.Select(rating => rating.Id));
        Assert.Equal(
            ["Birinchi izoh", "Ikkinchi izoh"],
            pendingRatings.Select(rating => rating.Comment));
    }
}
