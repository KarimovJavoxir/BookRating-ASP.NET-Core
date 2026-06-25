namespace BookRatingSystem.Application.Admin;

public sealed class BookRatingNotFoundException(Guid ratingId) : Exception($"Book rating with id '{ratingId}' was not found.")
{
    public Guid RatingId { get; } = ratingId;
}
