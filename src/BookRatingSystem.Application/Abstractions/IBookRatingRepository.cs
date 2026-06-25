using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookRatingRepository
{
    Task<BookRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookRatingProcessDto>> ListPendingCommentRatingsAsync(
        int limit,
        CancellationToken cancellationToken);
}
