using System.Text.Json.Serialization;

namespace BookRatingSystem.Infrastructure.Search;

public sealed class BookSearchDocument
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("publishedYear")]
    public int? PublishedYear { get; init; }

    [JsonPropertyName("coverImageUrl")]
    public string? CoverImageUrl { get; init; }

    [JsonPropertyName("averageRating")]
    public double AverageRating { get; init; }

    [JsonPropertyName("ratingsCount")]
    public int RatingsCount { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
