using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace BookRatingSystem.Api.Infrastructure;

public sealed class CommentProcessingWorker(
    IBackgroundTaskQueue<BookRatingProcessDto> queue,
    ILogger<CommentProcessingWorker> logger,
    IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    private const int PendingRatingsBatchSize = 1000;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await EnqueuePendingRatingsAsync(cancellationToken);

        await foreach (var job in queue.DequeueAllAsync(cancellationToken))
        {
            try
            {
                logger.LogInformation(
                    "Processing queued comment rating {RatingId}.",
                    job.Id);

                using var scope = serviceScopeFactory.CreateScope();
                var commentProcessingService = scope.ServiceProvider.GetRequiredService<ICommentProcessingService>();
                await commentProcessingService.ProcessAsync(job, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process queued comment rating {RatingId}.", job.Id);
            }
        }
    }

    private async Task EnqueuePendingRatingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBookRatingRepository>();
            var pendingRatings = await repository.ListPendingCommentRatingsAsync(
                PendingRatingsBatchSize,
                cancellationToken);

            foreach (var pendingRating in pendingRatings)
            {
                await queue.EnqueueAsync(pendingRating, cancellationToken);
            }

            logger.LogInformation(
                "Queued {PendingRatingsCount} pending comment ratings for background processing.",
                pendingRatings.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Failed to enqueue pending comment ratings during startup.");
        }
    }
}
