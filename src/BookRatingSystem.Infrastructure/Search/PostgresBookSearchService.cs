using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Search;

internal sealed class PostgresBookSearchService(BookRatingDbContext dbContext) : IBookSearchService
{
    public async Task<PagedResult<BookSearchResultDto>> SearchAsync(
        string query,
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
        {
            return new PagedResult<BookSearchResultDto>([], pagination.Page, pagination.PageSize, 0);
        }

        var pattern = $"%{trimmedQuery}%";
        var normalizedCategory = category?.Trim();

        var queryable = dbContext.Books
            .AsNoTracking()
            .Where(book => book.Status == BookStatus.Verified)
            .Where(book =>
                EF.Functions.ILike(book.Title, pattern) ||
                EF.Functions.ILike(book.Author, pattern) ||
                (book.Category != null && EF.Functions.ILike(book.Category, pattern)));

        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            queryable = queryable.Where(book => book.Category == normalizedCategory);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderBy(book => book.Title)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(book => new BookSearchResultDto(
                book.Id,
                book.Title,
                book.Author,
                book.Category,
                book.CoverImageUrl,
                book.Ratings.Count(rating => rating.Status == BookRatingStatus.Verified) == 0
                    ? 0m
                    : Math.Round(book.Ratings
                        .Where(rating => rating.Status == BookRatingStatus.Verified)
                        .Average(rating => (decimal)rating.Value), 2),
                book.Ratings.Count(rating => rating.Status == BookRatingStatus.Verified),
                book.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookSearchResultDto>(items, pagination.Page, pagination.PageSize, totalCount);
    }
}
