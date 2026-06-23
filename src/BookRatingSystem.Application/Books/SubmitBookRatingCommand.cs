namespace BookRatingSystem.Application.Books;

public sealed record SubmitBookRatingCommand(Guid UserId, int Value, string? Comment);
