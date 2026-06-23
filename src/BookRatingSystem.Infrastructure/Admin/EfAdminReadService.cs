using BookRatingSystem.Application.Admin;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Admin;

internal sealed class EfAdminReadService(BookRatingDbContext dbContext) : IAdminReadService
{
    public async Task<IReadOnlyList<AdminBookDto>> GetBooksAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .Select(book => new AdminBookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Category,
                book.Description,
                book.PublishedYear,
                book.CoverImageUrl,
                book.Status.ToString(),
                book.Ratings.Count == 0
                    ? 0m
                    : Math.Round(book.Ratings.Average(rating => (decimal)rating.Value), 2),
                book.Ratings.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .Select(user => new AdminUserDto(
                user.Id,
                user.Username,
                user.Email,
                user.ProfilePictureUrl,
                user.IsAdmin,
                user.CreatedAt,
                user.Ratings.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminBookRatingDto>> GetRatingsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.BookRatings
            .AsNoTracking()
            .OrderByDescending(rating => rating.CreatedAt)
            .Select(rating => new AdminBookRatingDto(
                rating.Id,
                rating.BookId,
                rating.Book == null ? string.Empty : rating.Book.Title,
                rating.UserId,
                rating.User == null ? string.Empty : rating.User.Username,
                rating.User == null ? null : rating.User.ProfilePictureUrl,
                rating.Value,
                rating.Comment,
                rating.Status.ToString(),
                rating.BanReason,
                rating.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var normalizedTo = NormalizeToEndOfDay(to);
        var totalBooks = await dbContext.Books.CountAsync(cancellationToken);
        var totalUsers = await dbContext.Users.CountAsync(cancellationToken);
        var totalRatings = await dbContext.BookRatings.CountAsync(cancellationToken);
        normalizedTo = normalizedTo?.UtcDateTime;
        from = from?.UtcDateTime;

        var booksInRangeQuery = ApplyBookDateRange(dbContext.Books.AsNoTracking(), from, normalizedTo);
        var ratingsInRangeQuery = ApplyRatingDateRange(dbContext.BookRatings.AsNoTracking(), from, normalizedTo);

        var booksAddedInRange = await booksInRangeQuery.CountAsync(cancellationToken);
        var ratingsAddedInRange = await ratingsInRangeQuery.CountAsync(cancellationToken);
        var averageRatingInRange = await ratingsInRangeQuery
            .Select(rating => (decimal?)rating.Value)
            .AverageAsync(cancellationToken);
        var recentRatings = await ratingsInRangeQuery
            .OrderByDescending(rating => rating.CreatedAt)
            .Take(5)
            .Select(rating => new AdminBookRatingDto(
                rating.Id,
                rating.BookId,
                rating.Book == null ? string.Empty : rating.Book.Title,
                rating.UserId,
                rating.User == null ? string.Empty : rating.User.Username,
                rating.User == null ? null : rating.User.ProfilePictureUrl,
                rating.Value,
                rating.Comment,
                rating.Status.ToString(),
                rating.BanReason,
                rating.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AdminDashboardDto(
            totalBooks,
            totalUsers,
            totalRatings,
            booksAddedInRange,
            ratingsAddedInRange,
            Math.Round(averageRatingInRange ?? 0m, 2, MidpointRounding.AwayFromZero),
            recentRatings);
    }

    private static IQueryable<Book> ApplyBookDateRange(
        IQueryable<Book> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(book => book.CreatedAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(book => book.CreatedAt <= to.Value);
        }

        return query;
    }

    private static IQueryable<BookRating> ApplyRatingDateRange(
        IQueryable<BookRating> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(rating => rating.CreatedAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(rating => rating.CreatedAt <= to.Value);
        }

        return query;
    }

    private static DateTimeOffset? NormalizeToEndOfDay(DateTimeOffset? value)
    {
        if (value is null || value.Value.TimeOfDay != TimeSpan.Zero)
        {
            return value;
        }

        return new DateTimeOffset(value.Value.Date.AddDays(1).AddTicks(-1), value.Value.Offset);
    }
}
