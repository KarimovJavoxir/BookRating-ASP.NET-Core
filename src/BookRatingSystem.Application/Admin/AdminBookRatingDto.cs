namespace BookRatingSystem.Application.Admin;

public sealed record AdminBookRatingDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid UserId,
    string Username,
    string? UserProfilePictureUrl,
    int Value,
    string? Comment,
    DateTimeOffset CreatedAt);
