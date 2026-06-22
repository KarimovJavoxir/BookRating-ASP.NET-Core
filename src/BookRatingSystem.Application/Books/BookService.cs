using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Application.Books;

public sealed class BookService(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IBookIndexingService bookIndexingService,
    ILogger<BookService>? logger = null) : IBookService
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

    public async Task<BookDetailsDto> CreateBookAsync(
        CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var book = Book.Create(
            Guid.NewGuid(),
            command.Title,
            command.Author,
            command.Category,
            command.Description,
            command.PublishedYear,
            command.CoverImageUrl,
            now);

        bookRepository.Add(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(book.Id, "create", cancellationToken);

        return BookMapper.ToDetails(book);
    }

    public async Task<BookDetailsDto> UpdateBookAsync(
        Guid bookId,
        UpdateBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(bookId);
        }

        book.Update(
            command.Title,
            command.Author,
            command.Category,
            command.Description,
            command.PublishedYear,
            command.CoverImageUrl,
            clock.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(book.Id, "update", cancellationToken);

        return BookMapper.ToDetails(book);
    }

    public async Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
        {
            throw new BookNotFoundException(bookId);
        }

        bookRepository.Delete(book);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryDeleteIndexedBookAsync(bookId, cancellationToken);
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
        await TryIndexBookAsync(book.Id, "rating submission", cancellationToken);

        return BookMapper.ToDetails(book);
    }

    private async Task TryIndexBookAsync(
        Guid bookId,
        string operationName,
        CancellationToken cancellationToken)
    {
        try
        {
            await bookIndexingService.IndexBookAsync(bookId, cancellationToken);
        }
        catch (Exception exception) when (CanContinueAfterIndexingFailure(exception, cancellationToken))
        {
            logger?.LogWarning(
                exception,
                "Could not synchronize book {BookId} with search index after {OperationName}. PostgreSQL changes were saved.",
                bookId,
                operationName);
        }
    }

    private async Task TryDeleteIndexedBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        try
        {
            await bookIndexingService.DeleteBookAsync(bookId, cancellationToken);
        }
        catch (Exception exception) when (CanContinueAfterIndexingFailure(exception, cancellationToken))
        {
            logger?.LogWarning(
                exception,
                "Could not delete book {BookId} from search index. PostgreSQL changes were saved.",
                bookId);
        }
    }

    private static bool CanContinueAfterIndexingFailure(Exception exception, CancellationToken cancellationToken)
    {
        return exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested;
    }
}
