using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Repositories;

internal sealed class EfBookRepository(BookRatingDbContext dbContext) : IBookRepository
{
    public async Task<PagedResult<Book>> ListAsync(PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var query = dbContext.Books
            .AsNoTracking()
            .Include(book => book.Ratings)
            .OrderBy(book => book.Title);

        return await ToPagedResultAsync(query, pagination, cancellationToken);
    }

    public async Task<PagedResult<Book>> ListVerifiedAsync(
        PaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        return await ListVerifiedAsync(pagination, category: null, cancellationToken);
    }

    public async Task<PagedResult<Book>> ListVerifiedAsync(
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Books
            .AsNoTracking()
            .Where(book => book.Status == BookStatus.Verified);

        var normalizedCategory = category?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            query = query.Where(book => book.Category == normalizedCategory);
        }

        var orderedQuery = query
            .Include(book => book.Ratings)
            .OrderBy(book => book.Title);

        return await ToPagedResultAsync(orderedQuery, pagination, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListVerifiedCategoriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Where(book => book.Status == BookStatus.Verified)
            .Where(book => book.Category != null)
            .Select(book => book.Category!)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListTopRatedVerifiedAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Where(book => book.Status == BookStatus.Verified)
            .Include(book => book.Ratings)
            .OrderByDescending(book => book.Ratings.Count(rating => rating.Status == BookRatingStatus.Verified) == 0
                ? 0m
                : book.Ratings
                    .Where(rating => rating.Status == BookRatingStatus.Verified)
                    .Average(rating => (decimal)rating.Value))
            .ThenByDescending(book => book.Ratings.Count(rating => rating.Status == BookRatingStatus.Verified))
            .ThenBy(book => book.Title)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Ratings)
            .ThenInclude(rating => rating.User)
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public void Add(Book book)
    {
        dbContext.Books.Add(book);
    }

    public void Delete(Book book)
    {
        dbContext.Books.Remove(book);
    }

    private static async Task<PagedResult<Book>> ToPagedResultAsync(
        IQueryable<Book> query,
        PaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Book>(items, pagination.Page, pagination.PageSize, totalCount);
    }
}
