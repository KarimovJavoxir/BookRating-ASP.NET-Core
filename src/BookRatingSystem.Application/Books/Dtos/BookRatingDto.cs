namespace BookRatingSystem.Application.Books.Dtos;

public sealed record BookRatingDto(
    Guid Id,
    Guid BookId,
    Guid UserId,
    int Value,
    string? Comment,
    DateTimeOffset CreatedAt);
