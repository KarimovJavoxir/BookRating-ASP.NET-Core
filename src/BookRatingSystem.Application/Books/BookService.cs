using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;

namespace BookRatingSystem.Application.Books;

public sealed class BookService(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : IBookService
{
    public async Task<IReadOnlyList<BookListItemDto>> GetBooksAsync(CancellationToken cancellationToken)
    {
        var books = await bookRepository.ListAsync(cancellationToken);

        return books
            .OrderBy(book => book.Title)
            .Select(BookMapper.ToListItem)
            .ToList();
    }

    public async Task<BookDetailsDto> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(id);
        }

        return BookMapper.ToDetails(book);
    }

    public async Task<BookDetailsDto> SubmitRatingAsync(
        Guid bookId,
        SubmitBookRatingCommand command,
        CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(bookId);
        }

        book.AddRating(command.Value, command.Comment, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BookMapper.ToDetails(book);
    }
}
