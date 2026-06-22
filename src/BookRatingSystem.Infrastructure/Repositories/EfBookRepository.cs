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

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Books
            .Include(book => book.Ratings)
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }
}
