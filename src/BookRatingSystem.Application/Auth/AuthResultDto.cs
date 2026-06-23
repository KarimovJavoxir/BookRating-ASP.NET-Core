namespace BookRatingSystem.Application.Auth;

public sealed record AuthResultDto(string Token, AuthUserDto User);
