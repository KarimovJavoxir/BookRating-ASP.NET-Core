namespace BookRatingSystem.Application.Admin;

public interface IAdminRatingModerationService
{
    Task AcceptRatingAsync(Guid ratingId, CancellationToken cancellationToken);

    Task BanRatingAsync(Guid ratingId, string? banReason, CancellationToken cancellationToken);
}
