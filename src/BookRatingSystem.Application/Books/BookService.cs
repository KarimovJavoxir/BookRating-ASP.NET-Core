using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Application.Books;

public sealed class BookService(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IBookIndexingService bookIndexingService,
    IBackgroundTaskQueue<BookRatingProcessDto> backgroundTaskQueue,
    ILogger<BookService>? logger = null) : IBookService
{
    private const int MaxTopRatedBooksLimit = 50;

    public async Task<PagedResult<BookListItemDto>> GetBooksAsync(
        PaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        return await GetBooksAsync(pagination, category: null, cancellationToken);
    }

    public async Task<PagedResult<BookListItemDto>> GetBooksAsync(
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken)
    {
        var books = await bookRepository.ListVerifiedAsync(pagination, category, cancellationToken);

        return new PagedResult<BookListItemDto>(
            books.Items.Select(BookMapper.ToListItem).ToList(),
            books.Page,
            books.PageSize,
            books.TotalCount);
    }

    public async Task<IReadOnlyList<string>> GetBookCategoriesAsync(CancellationToken cancellationToken)
    {
        return await bookRepository.ListVerifiedCategoriesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookListItemDto>> GetTopRatedBooksAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxTopRatedBooksLimit);
        var books = await bookRepository.ListTopRatedVerifiedAsync(normalizedLimit, cancellationToken);

        return books.Select(BookMapper.ToListItem).ToList();
    }

    public async Task<BookDetailsDto> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null || book.Status != BookStatus.Verified)
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
            now,
            command.Status);

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
            clock.UtcNow,
            command.Status);

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

        var rating = book.AddRating(command.UserId, command.Value, command.Comment, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(book.Id, "rating submission", cancellationToken);

        if (rating.Comment != null)
            await backgroundTaskQueue.EnqueueAsync(new BookRatingProcessDto(rating.Id, rating.Comment), cancellationToken);

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
