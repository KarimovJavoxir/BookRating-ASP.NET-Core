using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Repositories;

internal sealed class EfBookRepository(BookRatingDbContext dbContext) : IBookRepository
{
    public async Task<IReadOnlyList<Book>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Include(book => book.Ratings)
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListVerifiedAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Where(book => book.Status == BookStatus.Verified)
            .Include(book => book.Ratings)
            .OrderBy(book => book.Title)
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
}
