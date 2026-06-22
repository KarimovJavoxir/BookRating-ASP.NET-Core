using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Search;

internal sealed class PostgresBookSearchService(BookRatingDbContext dbContext) : IBookSearchService
{
    public async Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
        {
            return [];
        }

        var pattern = $"%{trimmedQuery}%";

        return await dbContext.Books
            .AsNoTracking()
            .Where(book =>
                EF.Functions.ILike(book.Title, pattern) ||
                EF.Functions.ILike(book.Author, pattern) ||
                (book.Category != null && EF.Functions.ILike(book.Category, pattern)))
            .OrderBy(book => book.Title)
            .Select(book => new BookSearchResultDto(
                book.Id,
                book.Title,
                book.Author,
                book.Category,
                book.CoverImageUrl,
                book.Ratings.Count == 0
                    ? 0m
                    : Math.Round(book.Ratings.Average(rating => (decimal)rating.Value), 2),
                book.Ratings.Count))
            .ToListAsync(cancellationToken);
    }
}
