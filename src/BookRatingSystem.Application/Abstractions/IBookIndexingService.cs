namespace BookRatingSystem.Application.Abstractions;

public interface IBookIndexingService
{
    Task EnsureIndexAsync(CancellationToken cancellationToken);

    Task IndexBookAsync(Guid bookId, CancellationToken cancellationToken);

    Task IndexAllBooksAsync(CancellationToken cancellationToken);

    Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken);
}
