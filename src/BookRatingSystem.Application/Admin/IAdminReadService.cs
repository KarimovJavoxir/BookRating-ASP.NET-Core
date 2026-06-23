namespace BookRatingSystem.Application.Admin;

public interface IAdminReadService
{
    Task<IReadOnlyList<AdminBookDto>> GetBooksAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminBookRatingDto>> GetRatingsAsync(CancellationToken cancellationToken);

    Task<AdminDashboardDto> GetDashboardAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);
}
