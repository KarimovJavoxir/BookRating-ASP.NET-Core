using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Books;

public sealed record UpdateBookCommand(
    string Title,
    string Author,
    string? Category,
    string? Description,
    int? PublishedYear,
    string? CoverImageUrl,
    BookStatus Status = BookStatus.Verified);
