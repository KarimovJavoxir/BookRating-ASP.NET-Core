using BookRatingSystem.Domain.Exceptions;

namespace BookRatingSystem.Domain.Entities;

public sealed class Book
{
    public const int MaxTitleLength = 200;
    public const int MaxAuthorLength = 150;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 2000;
    public const int MaxCoverImageUrlLength = 500;

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
        Title = NormalizeRequired(title, nameof(title), MaxTitleLength);
        Author = NormalizeRequired(author, nameof(author), MaxAuthorLength);
        Category = NormalizeOptional(category, nameof(category), MaxCategoryLength);
        Description = NormalizeOptional(description, nameof(description), MaxDescriptionLength);
        PublishedYear = publishedYear;
        CoverImageUrl = NormalizeOptional(coverImageUrl, nameof(coverImageUrl), MaxCoverImageUrlLength);
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

    public void Update(
        string title,
        string author,
        string? category,
        string? description,
        int? publishedYear,
        string? coverImageUrl,
        DateTimeOffset updatedAt)
    {
        Title = NormalizeRequired(title, nameof(title), MaxTitleLength);
        Author = NormalizeRequired(author, nameof(author), MaxAuthorLength);
        Category = NormalizeOptional(category, nameof(category), MaxCategoryLength);
        Description = NormalizeOptional(description, nameof(description), MaxDescriptionLength);
        PublishedYear = publishedYear;
        CoverImageUrl = NormalizeOptional(coverImageUrl, nameof(coverImageUrl), MaxCoverImageUrlLength);
        UpdatedAt = updatedAt;
    }

    public BookRating AddRating(Guid userId, int value, string? comment, DateTimeOffset createdAt)
    {
        var rating = BookRating.Create(Guid.NewGuid(), Id, userId, value, comment, createdAt);
        Ratings.Add(rating);
        UpdatedAt = createdAt;
        return rating;
    }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }
}
