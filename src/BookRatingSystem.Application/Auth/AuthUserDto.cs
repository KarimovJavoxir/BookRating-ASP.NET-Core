namespace BookRatingSystem.Application.Auth;

public sealed record AuthUserDto(Guid Id, string Username, string Email, bool IsAdmin);
