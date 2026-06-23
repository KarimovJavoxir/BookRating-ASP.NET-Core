namespace BookRatingSystem.Application.Auth;

public sealed record RegisterUserCommand(string Username, string Email, string Password);
