namespace BookRatingSystem.Application.Books;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string? Category,
    string? Description,
    int? PublishedYear,
    string? CoverImageUrl);
