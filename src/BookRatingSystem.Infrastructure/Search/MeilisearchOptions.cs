namespace BookRatingSystem.Infrastructure.Search;

internal sealed class MeilisearchOptions
{
    public const string SectionName = "Meilisearch";

    public string Url { get; set; } = "http://localhost:7700";

    public string ApiKey { get; set; } = string.Empty;

    public string BooksIndex { get; set; } = "books";

    public bool UsePostgresFallback { get; set; } = true;
}
