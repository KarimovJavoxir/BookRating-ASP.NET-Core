namespace BookRatingSystem.Application.Auth;

public sealed record AuthUserDto(Guid Id, string Username, string Email, string? ProfilePictureUrl, bool IsAdmin);
