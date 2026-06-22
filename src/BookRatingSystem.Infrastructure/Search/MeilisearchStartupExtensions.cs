using BookRatingSystem.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Infrastructure.Search;

public static class MeilisearchStartupExtensions
{
    public static async Task InitializeBookSearchIndexAsync(
        this IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = services.CreateScope();
            var indexingService = scope.ServiceProvider.GetRequiredService<IBookIndexingService>();

            await indexingService.EnsureIndexAsync(cancellationToken);
            await indexingService.IndexAllBooksAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Meilisearch index initialization did not complete. PostgreSQL remains the source of truth.");
        }
    }
}
