using BookRatingSystem.Domain.Exceptions;

namespace BookRatingSystem.Domain.Entities;

public sealed class BookRating
{
    public const int MinValue = 1;
    public const int MaxValue = 5;
    public const int MaxCommentLength = 500;

    private BookRating()
    {
    }

    private BookRating(Guid id, Guid bookId, int value, string? comment, DateTimeOffset createdAt)
    {
        if (value is < MinValue or > MaxValue)
        {
            throw new InvalidBookRatingException($"Rating value must be between {MinValue} and {MaxValue}.");
        }

        var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (normalizedComment?.Length > MaxCommentLength)
        {
            throw new InvalidBookRatingException($"Rating comment cannot exceed {MaxCommentLength} characters.");
        }

        Id = id;
        BookId = bookId;
        Value = value;
        Comment = normalizedComment;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public int Value { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Book? Book { get; private set; }

    public static BookRating Create(Guid id, Guid bookId, int value, string? comment, DateTimeOffset createdAt)
    {
        return new BookRating(id, bookId, value, comment, createdAt);
    }
}
