using BookRatingSystem.Domain.Exceptions;

namespace BookRatingSystem.Domain.Entities;

public sealed class BookRating
{
    public const int MinValue = 1;
    public const int MaxValue = 5;
    public const int MaxCommentLength = 500;
    public const int MaxBanReasonLength = 500;

    private BookRating()
    {
    }

    private BookRating(
        Guid id,
        Guid bookId,
        Guid userId,
        int value,
        string? comment,
        DateTimeOffset createdAt,
        BookRatingStatus status,
        string? banReason)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidBookRatingException("Rating user id is required.");
        }

        if (value is < MinValue or > MaxValue)
        {
            throw new InvalidBookRatingException($"Rating value must be between {MinValue} and {MaxValue}.");
        }

        var normalizedComment = NormalizeOptional(comment, MaxCommentLength, "comment");
        var normalizedBanReason = NormalizeOptional(banReason, MaxBanReasonLength, "ban reason");

        Id = id;
        BookId = bookId;
        UserId = userId;
        Value = value;
        Comment = normalizedComment;
        Status = status;
        BanReason = normalizedBanReason;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }
    public int Value { get; private set; }
    public string? Comment { get; private set; }
    public BookRatingStatus Status { get; private set; }
    public string? BanReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Book? Book { get; private set; }
    public User? User { get; private set; }

    public static BookRating Create(
        Guid id,
        Guid bookId,
        Guid userId,
        int value,
        string? comment,
        DateTimeOffset createdAt,
        BookRatingStatus status = BookRatingStatus.New,
        string? banReason = null)
    {
        return new BookRating(id, bookId, userId, value, comment, createdAt, status, banReason);
    }

    public void MarkVerified()
    {
        Status = BookRatingStatus.Verified;
        BanReason = null;
    }

    public void MarkBanned(string banReason)
    {
        var normalizedBanReason = NormalizeOptional(banReason, MaxBanReasonLength, "ban reason");
        if (normalizedBanReason is null)
        {
            throw new InvalidBookRatingException("Rating ban reason is required.");
        }

        Status = BookRatingStatus.Banned;
        BanReason = normalizedBanReason;
    }

    public void MarkNeedsHumanReview(string? reason)
    {
        Status = BookRatingStatus.NeedsHumanReview;
        BanReason = NormalizeOptional(reason, MaxBanReasonLength, "ban reason");
    }

    private static string? NormalizeOptional(string? value, int maxLength, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new InvalidBookRatingException($"Rating {displayName} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
