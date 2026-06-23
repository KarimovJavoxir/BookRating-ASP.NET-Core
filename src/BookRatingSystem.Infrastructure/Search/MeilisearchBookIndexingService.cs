using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Infrastructure.Persistence;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookRatingSystem.Infrastructure.Search;

internal sealed class MeilisearchBookIndexingService(
    BookRatingDbContext dbContext,
    MeilisearchClient client,
    IOptions<MeilisearchOptions> options,
    ILogger<MeilisearchBookIndexingService> logger) : IBookIndexingService
{
    private static readonly string[] SearchableAttributes =
    [
        "title",
        "author",
        "category",
        "description"
    ];

    private static readonly string[] FilterableAttributes =
    [
        "category",
        "publishedYear",
        "status"
    ];

    private static readonly string[] SortableAttributes =
    [
        "title",
        "author",
        "publishedYear",
        "averageRating",
        "ratingsCount"
    ];

    public Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        return RunSafelyAsync("ensure Meilisearch books index", async () =>
        {
            var index = await GetOrCreateIndexAsync(cancellationToken);

            await WaitForTaskAsync(
                await index.UpdateSearchableAttributesAsync(SearchableAttributes, cancellationToken),
                cancellationToken);
            await WaitForTaskAsync(
                await index.UpdateFilterableAttributesAsync(FilterableAttributes, cancellationToken),
                cancellationToken);
            await WaitForTaskAsync(
                await index.UpdateSortableAttributesAsync(SortableAttributes, cancellationToken),
                cancellationToken);
        }, cancellationToken);
    }

    public Task IndexBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return RunSafelyAsync($"index book {bookId}", async () =>
        {
            var book = await dbContext.Books
                .AsNoTracking()
                .Include(currentBook => currentBook.Ratings)
                .FirstOrDefaultAsync(currentBook => currentBook.Id == bookId, cancellationToken);

            if (book is null)
            {
                await DeleteBookCoreAsync(bookId, cancellationToken);
                return;
            }

            var index = client.Index(options.Value.BooksIndex);
            var document = BookSearchDocumentMapper.ToDocument(book);
            await WaitForTaskAsync(
                await index.AddDocumentsAsync([document], "id", cancellationToken),
                cancellationToken);
        }, cancellationToken);
    }

    public Task IndexAllBooksAsync(CancellationToken cancellationToken)
    {
        return RunSafelyAsync("index all books", async () =>
        {
            var books = await dbContext.Books
                .AsNoTracking()
                .Include(book => book.Ratings)
                .OrderBy(book => book.Title)
                .ToListAsync(cancellationToken);

            if (books.Count == 0)
            {
                return;
            }

            var documents = books
                .Select(BookSearchDocumentMapper.ToDocument)
                .ToList();

            var index = client.Index(options.Value.BooksIndex);
            await WaitForTaskAsync(
                await index.AddDocumentsAsync(documents, "id", cancellationToken),
                cancellationToken);
        }, cancellationToken);
    }

    public Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return RunSafelyAsync($"delete indexed book {bookId}", () => DeleteBookCoreAsync(bookId, cancellationToken), cancellationToken);
    }

    private async Task<Meilisearch.Index> GetOrCreateIndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetIndexAsync(options.Value.BooksIndex, cancellationToken);
        }
        catch (MeilisearchApiError exception) when (exception.Code == "index_not_found")
        {
            var task = await client.CreateIndexAsync(options.Value.BooksIndex, "id", cancellationToken);
            await WaitForTaskAsync(task, cancellationToken);
            return client.Index(options.Value.BooksIndex);
        }
    }

    private async Task DeleteBookCoreAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var index = client.Index(options.Value.BooksIndex);
        await WaitForTaskAsync(
            await index.DeleteOneDocumentAsync(bookId.ToString(), cancellationToken),
            cancellationToken);
    }

    private Task WaitForTaskAsync(TaskInfo task, CancellationToken cancellationToken)
    {
        return client.WaitForTaskAsync(task.TaskUid, cancellationToken: cancellationToken);
    }

    private async Task RunSafelyAsync(
        string operationName,
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            await operation();
        }
        catch (Exception exception) when (IsNonCancellationFailure(exception, cancellationToken))
        {
            logger.LogWarning(exception, "Could not {OperationName}.", operationName);
        }
    }

    private static bool IsNonCancellationFailure(Exception exception, CancellationToken cancellationToken)
    {
        return exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested;
    }
}
