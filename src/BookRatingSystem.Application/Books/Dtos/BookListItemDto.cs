namespace BookRatingSystem.Application.Books.Dtos;

public sealed record BookListItemDto(
    Guid Id,
    string Title,
    string Author,
    string? Category,
    string? CoverImageUrl,
    decimal AverageRating,
    int RatingsCount);
