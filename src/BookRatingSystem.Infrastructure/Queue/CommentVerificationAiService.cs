using System.Text.Json;
using System.Text.Json.Serialization;
using BookRatingSystem.Application.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace BookRatingSystem.Infrastructure.Queue;

public sealed class CommentVerificationAiService(
    IChatClient chatClient,
    ILogger<CommentVerificationAiService> logger)
    : ICommentVerificationAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<CommentVerificationResult> VerifyAsync(
        string comment,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return new CommentVerificationResult
            {
                ShouldBeBanned = false,
                BanExplanation = null,
                Confidence = 100
            };
        }

        var zeroConfidence = new CommentVerificationResult
        {
            ShouldBeBanned = false,
            BanExplanation = null,
            Confidence = 0
        };

        comment = comment.Trim();

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, """
                You are a strict book-rating comment verification service.

                Your job:
                - Approve useful comments about a book.
                - Reject spam, insults, hate, sexual content, personal data, irrelevant text, fake-looking comments, and comments not about the book.
                - Return ShouldBeBanned as true only when the comment must be banned.
                - Return Confidence from 0 to 100.
                - If the comment is unclear but not dangerous, keep ShouldBeBanned false and set Confidence below 50 for human review.
                - Keep the reason short and practical.
                - Your explanation should be in Uzbek language.
                """),

            new(ChatRole.User, $$"""
                Verify this book rating comment: 
                ```{{comment}}```
                """)
        };

        try
        {
            var response = await chatClient.GetResponseAsync(
                messages,
                new ChatOptions
                {
                    Temperature = 0,
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<CommentVerificationResult>()
                },
                cancellationToken);

            var result = JsonSerializer.Deserialize<CommentVerificationResult>(response.Text, JsonOptions);

            return result is null ? zeroConfidence : NormalizeResult(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify comment with AI.");

            return zeroConfidence;
        }
    }

    private static CommentVerificationResult NormalizeResult(CommentVerificationResult result)
    {
        return result with
        {
            Confidence = Math.Clamp(result.Confidence, 0, 100)
        };
    }
}
