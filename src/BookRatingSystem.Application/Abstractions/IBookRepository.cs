using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookRepository
{
    Task<PagedResult<Book>> ListAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<Book>> ListVerifiedAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<Book>> ListVerifiedAsync(
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListVerifiedCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Book>> ListTopRatedVerifiedAsync(int limit, CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Book book);

    void Delete(Book book);
}
