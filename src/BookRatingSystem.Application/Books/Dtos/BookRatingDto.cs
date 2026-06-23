namespace BookRatingSystem.Application.Books.Dtos;

public sealed record BookRatingDto(
    Guid Id,
    Guid BookId,
    Guid UserId,
    string? Username,
    string? UserProfilePictureUrl,
    int Value,
    string? Comment,
    string Status,
    string? BanReason,
    DateTimeOffset CreatedAt);
