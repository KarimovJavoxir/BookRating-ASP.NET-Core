using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;

namespace BookRatingSystem.Application.Books;

public interface IBookService
{
    Task<PagedResult<BookListItemDto>> GetBooksAsync(PaginationQuery pagination, CancellationToken cancellationToken);

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
