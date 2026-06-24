using System.Data.Common;
using BookRatingSystem.Application.Admin;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Tests;

public sealed class EfAdminReadServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_completes_with_a_single_scoped_db_context()
    {
        var options = new DbContextOptionsBuilder<BookRatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BookRatingDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var createdAt = new DateTimeOffset(2026, 6, 23, 10, 0, 0, TimeSpan.Zero);
        var bookId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
        var userId = Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb");
        var ratingId = Guid.Parse("cccccccc-cccc-4ccc-cccc-cccccccccccc");
        var rating = BookRating.Create(
            ratingId,
            bookId,
            userId,
            value: 5,
            comment: "Foydali kitob",
            createdAt);

        dbContext.Books.Add(Book.Create(
            bookId,
            "Algoritmlar",
            "A. Muallif",
            "Dasturlash",
            null,
            2026,
            null,
            createdAt));
        dbContext.Users.Add(User.Create(
            userId,
            "dashboard-test-admin",
            "dashboard-test-admin@example.com",
            "hash",
            null,
            isAdmin: true,
            createdAt));
        dbContext.BookRatings.Add(rating);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var dashboard = await service.GetDashboardAsync(from: null, to: null, CancellationToken.None);

        Assert.True(dashboard.TotalBooks >= 1);
        Assert.True(dashboard.TotalUsers >= 1);
        Assert.True(dashboard.TotalRatings >= 1);
        Assert.True(dashboard.BooksAddedInRange >= 1);
        Assert.True(dashboard.RatingsAddedInRange >= 1);
        Assert.Contains(dashboard.RecentRatings, rating => rating.Id == ratingId);
    }

    [Fact]
    public async Task Admin_lists_return_requested_page_with_total_metadata()
    {
        var options = new DbContextOptionsBuilder<BookRatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new BookRatingDbContext(options);

        var createdAt = new DateTimeOffset(2026, 6, 24, 10, 0, 0, TimeSpan.Zero);
        for (var index = 1; index <= 3; index++)
        {
            var bookId = Guid.Parse($"aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaa{index}");
            var userId = Guid.Parse($"bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbb{index}");
            dbContext.Books.Add(Book.Create(
                bookId,
                $"Kitob {index}",
                "A. Muallif",
                "Dasturlash",
                null,
                2026,
                null,
                createdAt.AddMinutes(index)));
            dbContext.Users.Add(User.Create(
                userId,
                $"user0{index}",
                $"user0{index}@example.com",
                "hash",
                null,
                isAdmin: false,
                createdAt.AddMinutes(index)));
            dbContext.BookRatings.Add(BookRating.Create(
                Guid.Parse($"cccccccc-cccc-4ccc-cccc-ccccccccccc{index}"),
                bookId,
                userId,
                value: index,
                comment: $"Izoh {index}",
                createdAt.AddMinutes(index)));
        }

        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);
        var pagination = new PaginationQuery(page: 2, pageSize: 1);

        var books = await service.GetBooksAsync(pagination, CancellationToken.None);
        var users = await service.GetUsersAsync(pagination, CancellationToken.None);
        var ratings = await service.GetRatingsAsync(pagination, CancellationToken.None);

        Assert.Equal(3, books.TotalCount);
        Assert.Equal(3, users.TotalCount);
        Assert.Equal(3, ratings.TotalCount);
        Assert.Equal(3, books.TotalPages);
        Assert.Equal("Kitob 2", Assert.Single(books.Items).Title);
        Assert.Equal("user02", Assert.Single(users.Items).Username);
        Assert.Equal("Kitob 2", Assert.Single(ratings.Items).BookTitle);
    }

    private static IAdminReadService CreateService(BookRatingDbContext dbContext)
    {
        var serviceType = typeof(BookRatingDbContext).Assembly.GetType(
            "BookRatingSystem.Infrastructure.Admin.EfAdminReadService",
            throwOnError: true)!;
        return (IAdminReadService)Activator.CreateInstance(serviceType, dbContext)!;
    }
}
