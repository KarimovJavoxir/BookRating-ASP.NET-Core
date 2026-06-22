using BookRatingSystem.Application.Abstractions;

namespace BookRatingSystem.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
