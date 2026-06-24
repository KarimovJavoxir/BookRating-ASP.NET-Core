using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookRepository
{
    Task<PagedResult<Book>> ListAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<Book>> ListVerifiedAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Book book);

    void Delete(Book book);
}
