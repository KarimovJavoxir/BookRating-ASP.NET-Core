using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Tests;

public sealed class EfBookRepositoryOrderingTests
{
    [Fact]
    public async Task ListVerifiedAsync_orders_by_verified_average_rating_before_pagination()
    {
        var options = new DbContextOptionsBuilder<BookRatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BookRatingDbContext(options);
        var createdAt = new DateTimeOffset(2026, 6, 27, 10, 0, 0, TimeSpan.Zero);
        var alpha = CreateBook("10000000-0000-0000-0000-000000000001", "Alpha", createdAt);
        var beta = CreateBook("10000000-0000-0000-0000-000000000002", "Beta", createdAt);
        var delta = CreateBook("10000000-0000-0000-0000-000000000003", "Delta", createdAt);
        var echo = CreateBook("10000000-0000-0000-0000-000000000004", "Echo", createdAt);
        var zulu = CreateBook("10000000-0000-0000-0000-000000000005", "Zulu", createdAt);

        dbContext.Books.AddRange(alpha, beta, delta, echo, zulu);
        dbContext.BookRatings.AddRange(
            CreateRating("20000000-0000-0000-0000-000000000001", alpha.Id, 5, BookRatingStatus.New, createdAt),
            CreateRating("20000000-0000-0000-0000-000000000002", beta.Id, 5, BookRatingStatus.Verified, createdAt),
            CreateRating("20000000-0000-0000-0000-000000000003", delta.Id, 4, BookRatingStatus.Verified, createdAt),
            CreateRating("20000000-0000-0000-0000-000000000004", echo.Id, 4, BookRatingStatus.Verified, createdAt),
            CreateRating("20000000-0000-0000-0000-000000000005", zulu.Id, 5, BookRatingStatus.Verified, createdAt),
            CreateRating("20000000-0000-0000-0000-000000000006", zulu.Id, 5, BookRatingStatus.Verified, createdAt));
        await dbContext.SaveChangesAsync();

        var repository = CreateRepository(dbContext);

        var result = await repository.ListVerifiedAsync(
            new PaginationQuery(page: 1, pageSize: 10),
            CancellationToken.None);

        Assert.Equal(["Zulu", "Beta", "Delta", "Echo", "Alpha"], result.Items.Select(book => book.Title));
    }

    private static Book CreateBook(string id, string title, DateTimeOffset createdAt)
    {
        return Book.Create(Guid.Parse(id), title, "Author", null, null, 2026, null, createdAt);
    }

    private static BookRating CreateRating(
        string id,
        Guid bookId,
        int value,
        BookRatingStatus status,
        DateTimeOffset createdAt)
    {
        return BookRating.Create(
            Guid.Parse(id),
            bookId,
            Guid.NewGuid(),
            value,
            null,
            createdAt,
            status);
    }

    private static IBookRepository CreateRepository(BookRatingDbContext dbContext)
    {
        var repositoryType = typeof(BookRatingDbContext).Assembly.GetType(
            "BookRatingSystem.Infrastructure.Repositories.EfBookRepository",
            throwOnError: true)!;
        return (IBookRepository)Activator.CreateInstance(repositoryType, dbContext)!;
    }
}
