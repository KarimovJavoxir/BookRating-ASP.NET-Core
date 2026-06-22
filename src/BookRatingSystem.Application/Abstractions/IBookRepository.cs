using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> ListAsync(CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
