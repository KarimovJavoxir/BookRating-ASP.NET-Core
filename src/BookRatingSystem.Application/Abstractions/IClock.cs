namespace BookRatingSystem.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
