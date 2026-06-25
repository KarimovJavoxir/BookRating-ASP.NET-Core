namespace BookRatingSystem.Application.Abstractions;

public interface ICommentVerificationAiService
{
    Task<CommentVerificationResult> VerifyAsync(
        string comment,
        CancellationToken cancellationToken = default);
}

public sealed record CommentVerificationResult
{
    public required bool ShouldBeBanned { get; init; }

    public string? BanExplanation { get; init; }

    public required int Confidence { get; init; }
}