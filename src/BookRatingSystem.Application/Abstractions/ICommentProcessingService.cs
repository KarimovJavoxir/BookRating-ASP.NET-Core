using BookRatingSystem.Application.Books.Dtos;

namespace BookRatingSystem.Application.Abstractions;

public interface ICommentProcessingService
{
    Task ProcessAsync(BookRatingProcessDto job, CancellationToken cancellationToken);
}
