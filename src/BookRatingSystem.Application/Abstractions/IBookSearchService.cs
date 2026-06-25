using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;

namespace BookRatingSystem.Application.Abstractions;

public interface IBookSearchService
{
    Task<PagedResult<BookSearchResultDto>> SearchAsync(
        string query,
        PaginationQuery pagination,
        string? category,
        CancellationToken cancellationToken);
}
