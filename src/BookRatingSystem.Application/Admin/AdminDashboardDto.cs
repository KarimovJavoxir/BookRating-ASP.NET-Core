namespace BookRatingSystem.Application.Admin;

public sealed record AdminDashboardDto(
    int TotalBooks,
    int TotalUsers,
    int TotalRatings,
    int BooksAddedInRange,
    int RatingsAddedInRange,
    decimal AverageRatingInRange,
    IReadOnlyList<AdminBookRatingDto> RecentRatings);
