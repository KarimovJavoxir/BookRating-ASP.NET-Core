using System.Net;
using System.Text;
using System.Text.Json;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Infrastructure.Persistence;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BookRatingSystem.Tests;

public sealed class BookSearchOrderingTests
{
    [Fact]
    public async Task Meilisearch_search_orders_by_average_rating_with_stable_ties()
    {
        var handler = new RecordingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:7700"),
        };
        var client = new MeilisearchClient(httpClient, "test-key");

        var dbOptions = new DbContextOptionsBuilder<BookRatingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new BookRatingDbContext(dbOptions);
        var searchService = CreateSearchService(client, dbContext);

        await searchService.SearchAsync(
            "security",
            new PaginationQuery(page: 1, pageSize: 10),
            category: null,
            CancellationToken.None);

        using var request = JsonDocument.Parse(Assert.IsType<string>(handler.RequestContent));
        var sort = request.RootElement
            .GetProperty("sort")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();

        Assert.Equal(["averageRating:desc", "ratingsCount:desc", "title:asc"], sort);
    }

    private static IBookSearchService CreateSearchService(
        MeilisearchClient client,
        BookRatingDbContext dbContext)
    {
        var assembly = typeof(BookRatingDbContext).Assembly;
        var optionsType = assembly.GetType(
            "BookRatingSystem.Infrastructure.Search.MeilisearchOptions",
            throwOnError: true)!;
        var optionsValue = Activator.CreateInstance(optionsType)!;
        optionsType.GetProperty("BooksIndex")!.SetValue(optionsValue, "books");

        var optionsWrapperType = typeof(OptionsWrapper<>).MakeGenericType(optionsType);
        var options = Activator.CreateInstance(optionsWrapperType, optionsValue)!;

        var postgresType = assembly.GetType(
            "BookRatingSystem.Infrastructure.Search.PostgresBookSearchService",
            throwOnError: true)!;
        var postgresFallback = Activator.CreateInstance(postgresType, dbContext)!;

        var serviceType = assembly.GetType(
            "BookRatingSystem.Infrastructure.Search.MeilisearchBookSearchService",
            throwOnError: true)!;
        var loggerType = typeof(NullLogger<>).MakeGenericType(serviceType);
        var logger = Activator.CreateInstance(loggerType)!;

        return (IBookSearchService)Activator.CreateInstance(
            serviceType,
            client,
            options,
            postgresFallback,
            logger)!;
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public string? RequestContent { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestContent = await request.Content!.ReadAsStringAsync(cancellationToken);

            const string response = """
                {
                  "hits": [],
                  "query": "security",
                  "processingTimeMs": 1,
                  "hitsPerPage": 10,
                  "page": 1,
                  "totalPages": 0,
                  "totalHits": 0
                }
                """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }
}
