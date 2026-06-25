using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookService
{
    Task<PagedResult<BookListItemDto>> GetBooksAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<BookListItemDto>> GetBooksAsync(
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetBookCategoriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<BookListItemDto>> GetTopRatedBooksAsync(int limit, CancellationToken cancellationToken);

    Task<BookDetailsDto> GetBookByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<BookDetailsDto> CreateBookAsync(CreateBookCommand command, CancellationToken cancellationToken);

    Task<BookDetailsDto> UpdateBookAsync(
        Guid bookId,
        UpdateBookCommand command,
        CancellationToken cancellationToken);

    Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken);

    Task<BookDetailsDto> SubmitRatingAsync(
        Guid bookId,
        SubmitBookRatingCommand command,
        CancellationToken cancellationToken);
}
