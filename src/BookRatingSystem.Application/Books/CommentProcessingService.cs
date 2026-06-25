using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Application.Books;

public sealed class CommentProcessingService(
    IBookRatingRepository bookRatingRepository,
    IUnitOfWork unitOfWork,
    ICommentVerificationAiService commentVerificationAiService,
    IBookIndexingService bookIndexingService,
    ILogger<CommentProcessingService>? logger = null)
    : ICommentProcessingService
{
    private const int MinimumAutomaticDecisionConfidence = 50;
    private const string DefaultBanReason = "AI tekshiruv izohni avtomatik tasdiqlamadi.";
    private const string LowConfidenceReason = "AI tekshiruv natijasi yetarli darajada ishonchli emas.";

    public async Task ProcessAsync(BookRatingProcessDto job, CancellationToken cancellationToken)
    {
        var rating = await bookRatingRepository.GetByIdAsync(job.Id, cancellationToken);
        if (rating is null)
        {
            logger?.LogWarning("Queued comment rating {RatingId} was not found.", job.Id);
            return;
        }

        if (rating.Status != BookRatingStatus.New)
        {
            logger?.LogInformation(
                "Skipping comment rating {RatingId} because its status is already {Status}.",
                rating.Id,
                rating.Status);
            return;
        }

        var result = await commentVerificationAiService.VerifyAsync(job.Comment, cancellationToken);
        ApplyModerationResult(rating, result);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(rating.BookId, cancellationToken);
    }

    private static void ApplyModerationResult(
        BookRating rating,
        CommentVerificationResult result)
    {
        if (result.Confidence < MinimumAutomaticDecisionConfidence)
        {
            rating.MarkNeedsHumanReview(result.BanExplanation ?? LowConfidenceReason);
            return;
        }

        if (result.ShouldBeBanned)
        {
            rating.MarkBanned(result.BanExplanation ?? DefaultBanReason);
            return;
        }

        rating.MarkVerified();
    }

    private async Task TryIndexBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        try
        {
            await bookIndexingService.IndexBookAsync(bookId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning(
                exception,
                "Could not synchronize book {BookId} with search index after comment processing. PostgreSQL changes were saved.",
                bookId);
        }
    }
}
