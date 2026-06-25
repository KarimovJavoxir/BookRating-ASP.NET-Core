using BookRatingSystem.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Application.Admin;

public sealed class AdminRatingModerationService(
    IBookRatingRepository bookRatingRepository,
    IUnitOfWork unitOfWork,
    IBookIndexingService bookIndexingService,
    ILogger<AdminRatingModerationService>? logger = null) : IAdminRatingModerationService
{
    private const string DefaultBanReason = "Admin tomonidan rad etildi";

    public async Task AcceptRatingAsync(Guid ratingId, CancellationToken cancellationToken)
    {
        var rating = await bookRatingRepository.GetByIdAsync(ratingId, cancellationToken);
        if (rating is null)
        {
            throw new BookRatingNotFoundException(ratingId);
        }

        rating.MarkVerified();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(rating.BookId, rating.Id, cancellationToken);
    }

    public async Task BanRatingAsync(Guid ratingId, string? banReason, CancellationToken cancellationToken)
    {
        var rating = await bookRatingRepository.GetByIdAsync(ratingId, cancellationToken);
        if (rating is null)
        {
            throw new BookRatingNotFoundException(ratingId);
        }

        rating.MarkBanned(string.IsNullOrWhiteSpace(banReason) ? DefaultBanReason : banReason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await TryIndexBookAsync(rating.BookId, rating.Id, cancellationToken);
    }

    private async Task TryIndexBookAsync(Guid bookId, Guid ratingId, CancellationToken cancellationToken)
    {
        try
        {
            await bookIndexingService.IndexBookAsync(bookId, cancellationToken);
        }
        catch (Exception exception) when (CanContinueAfterIndexingFailure(exception, cancellationToken))
        {
            logger?.LogWarning(
                exception,
                "Could not synchronize book {BookId} with search index after moderating rating {RatingId}. PostgreSQL changes were saved.",
                bookId,
                ratingId);
        }
    }

    private static bool CanContinueAfterIndexingFailure(Exception exception, CancellationToken cancellationToken)
    {
        return exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested;
    }
}
