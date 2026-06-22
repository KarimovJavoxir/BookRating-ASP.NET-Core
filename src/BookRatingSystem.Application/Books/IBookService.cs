using BookRatingSystem.Application.Books.Dtos;

namespace BookRatingSystem.Application.Books;

public interface IBookService
{
    Task<IReadOnlyList<BookListItemDto>> GetBooksAsync(CancellationToken cancellationToken);

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
