namespace BookRatingSystem.Application.Admin;

public sealed record AdminUserDto(
    Guid Id,
    string Username,
    string Email,
    string? ProfilePictureUrl,
    bool IsAdmin,
    DateTimeOffset CreatedAt,
    int RatingsCount);
