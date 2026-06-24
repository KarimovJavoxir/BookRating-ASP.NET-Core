using BookRatingSystem.Application.Common;

namespace BookRatingSystem.Application.Admin;

public interface IAdminReadService
{
    Task<PagedResult<AdminBookDto>> GetBooksAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<AdminUserDto>> GetUsersAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<PagedResult<AdminBookRatingDto>> GetRatingsAsync(PaginationQuery pagination, CancellationToken cancellationToken);

    Task<AdminDashboardDto> GetDashboardAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);
}
