namespace BookRatingSystem.Api.Contracts;

public sealed class OpenAiOptions
{
    public required string ApiKey { get; init; }

    public string Model { get; init; } = "gpt-5";
}
