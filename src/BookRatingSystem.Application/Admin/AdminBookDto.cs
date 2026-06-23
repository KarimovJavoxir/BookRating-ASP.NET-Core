namespace BookRatingSystem.Application.Admin;

public sealed record AdminBookDto(
    Guid Id,
    string Title,
    string Author,
    string? Category,
    string? Description,
    int? PublishedYear,
    string? CoverImageUrl,
    string Status,
    decimal AverageRating,
    int RatingsCount);
