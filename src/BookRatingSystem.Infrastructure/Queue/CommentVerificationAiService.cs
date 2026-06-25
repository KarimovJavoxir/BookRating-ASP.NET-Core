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
                You are a strict book-rating comment verification service. Your main job is to do all of human's work. Yet, you shouldn't ban genuine Book Ratings.
                
                Rules:
                - Approve only meaningful comments clearly about the book, its content, quality, usefulness, topic, language, author, or reading experience.
                - Ban spam, insults, profanity, hate, sexual content, threats, ads, links, personal data, phone numbers, usernames, private info, random text, emoji-only text, keyboard spam, irrelevant text, and fake-looking comments.
                - Ban very generic comments like "zo'r", "yaxshi", "ok", "gap yo'q", "super", "👍", unless they include useful book-related detail.
                - Ban comments not about the book.
                
                Confidence:
                - Confidence means how sure you are about your decision.
                - Obvious trash, spam, irrelevant, meaningless, or generic comments must be banned with high confidence.
                - Use confidence below 50 only when the comment might be a valid book review but is genuinely unclear.
                - Do not use low confidence for clearly bad comments.
                
                banExplanation:
                - Uzbek language.
                - Short and practical.
                - null when approved.
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
                    Reasoning = new ReasoningOptions
                    {
                        Effort = ReasoningEffort.Medium,
                        Output = ReasoningOutput.None
                    },
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
