using BookRatingSystem.Application.Books.Dtos;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookSearchService
{
    Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(string query, CancellationToken cancellationToken);
}
