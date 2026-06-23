using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookRatingSystem.Infrastructure.Search;

internal sealed class MeilisearchBookSearchService(
    MeilisearchClient client,
    IOptions<MeilisearchOptions> options,
    PostgresBookSearchService postgresFallback,
    ILogger<MeilisearchBookSearchService> logger) : IBookSearchService
{
    public async Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
        {
            return [];
        }

        try
        {
            var index = client.Index(options.Value.BooksIndex);
            var result = await index.SearchAsync<BookSearchDocument>(
                trimmedQuery,
                new SearchQuery
                {
                    Limit = 50,
                    Filter = "status = Verified"
                },
                cancellationToken);

            return result.Hits
                .Select(BookSearchDocumentMapper.ToSearchResult)
                .ToList();
        }
        catch (Exception exception) when (IsMeilisearchFailure(exception, cancellationToken))
        {
            if (!options.Value.UsePostgresFallback)
            {
                throw new BookSearchUnavailableException("Meilisearch qidiruv servisi vaqtincha ishlamayapti.", exception);
            }

            logger.LogWarning(
                exception,
                "Meilisearch search failed. Falling back to PostgreSQL search for query {Query}.",
                trimmedQuery);

            return await postgresFallback.SearchAsync(trimmedQuery, cancellationToken);
        }
    }

    private static bool IsMeilisearchFailure(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return exception is MeilisearchApiError
            or MeilisearchCommunicationError
            or MeilisearchTimeoutError
            or HttpRequestException
            or TaskCanceledException;
    }
}
