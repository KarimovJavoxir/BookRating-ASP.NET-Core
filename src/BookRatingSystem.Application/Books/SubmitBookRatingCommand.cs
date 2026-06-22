namespace BookRatingSystem.Application.Books;

public sealed record SubmitBookRatingCommand(int Value, string? Comment);
