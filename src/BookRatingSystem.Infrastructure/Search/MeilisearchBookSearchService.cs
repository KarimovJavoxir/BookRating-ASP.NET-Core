using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
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
    public async Task<PagedResult<BookSearchResultDto>> SearchAsync(
        string query,
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
        {
            return new PagedResult<BookSearchResultDto>([], pagination.Page, pagination.PageSize, 0);
        }

        try
        {
            var index = client.Index(options.Value.BooksIndex);
            var result = await index.SearchAsync<BookSearchDocument>(
                trimmedQuery,
                new SearchQuery
                {
                    HitsPerPage = pagination.PageSize,
                    Page = pagination.Page,
                    Filter = BuildFilter(category)
                },
                cancellationToken);

            var items = result.Hits
                .Select(BookSearchDocumentMapper.ToSearchResult)
                .ToList();

            return new PagedResult<BookSearchResultDto>(
                items,
                pagination.Page,
                pagination.PageSize,
                GetTotalHits(result));
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

            return await postgresFallback.SearchAsync(trimmedQuery, pagination, category, cancellationToken);
        }
    }

    private static string BuildFilter(string? category)
    {
        var normalizedCategory = category?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCategory))
        {
            return "status = Verified";
        }

        return $"status = Verified AND category = \"{EscapeFilterValue(normalizedCategory)}\"";
    }

    private static string EscapeFilterValue(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static int GetTotalHits(ISearchable<BookSearchDocument> result)
    {
        return result switch
        {
            PaginatedSearchResult<BookSearchDocument> paginatedResult => paginatedResult.TotalHits,
            SearchResult<BookSearchDocument> searchResult => searchResult.EstimatedTotalHits,
            _ => result.Hits.Count
        };
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
