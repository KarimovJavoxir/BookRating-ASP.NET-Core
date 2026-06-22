namespace BookRatingSystem.Application.Books.Dtos;

public sealed record BookDetailsDto(
    Guid Id,
    string Title,
    string Author,
    string? Category,
    string? Description,
    int? PublishedYear,
    string? CoverImageUrl,
    decimal AverageRating,
    int RatingsCount,
    IReadOnlyList<BookRatingDto> RecentRatings);
