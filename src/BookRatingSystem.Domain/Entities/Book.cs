using BookRatingSystem.Domain.Exceptions;

namespace BookRatingSystem.Domain.Entities;

public sealed class Book
{
    private Book()
    {
    }

    private Book(
        Guid id,
        string title,
        string author,
        string? category,
        string? description,
        int? publishedYear,
        string? coverImageUrl,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = NormalizeRequired(title, nameof(title));
        Author = NormalizeRequired(author, nameof(author));
        Category = NormalizeOptional(category);
        Description = NormalizeOptional(description);
        PublishedYear = publishedYear;
        CoverImageUrl = NormalizeOptional(coverImageUrl);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public string? Description { get; private set; }
    public int? PublishedYear { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<BookRating> Ratings { get; private set; } = [];

    public static Book Create(
        Guid id,
        string title,
        string author,
        string? category,
        string? description,
        int? publishedYear,
        string? coverImageUrl,
        DateTimeOffset createdAt)
    {
        return new Book(id, title, author, category, description, publishedYear, coverImageUrl, createdAt);
    }

    public BookRating AddRating(int value, string? comment, DateTimeOffset createdAt)
    {
        var rating = BookRating.Create(Guid.NewGuid(), Id, value, comment, createdAt);
        Ratings.Add(rating);
        UpdatedAt = createdAt;
        return rating;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
