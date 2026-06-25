using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Repositories;

internal sealed class EfBookRatingRepository(BookRatingDbContext dbContext) : IBookRatingRepository
{
    public Task<BookRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.BookRatings
            .FirstOrDefaultAsync(rating => rating.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<BookRatingProcessDto>> ListPendingCommentRatingsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 1000);

        return await dbContext.BookRatings
            .AsNoTracking()
            .Where(rating => rating.Status == BookRatingStatus.New)
            .Where(rating => rating.Comment != null)
            .OrderBy(rating => rating.CreatedAt)
            .Take(normalizedLimit)
            .Select(rating => new BookRatingProcessDto(rating.Id, rating.Comment!))
            .ToListAsync(cancellationToken);
    }
}
