using BookRatingSystem.Api.Controllers;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Tests;

public sealed class BooksControllerTests
{
    [Fact]
    public async Task SearchBooks_returns_requested_search_page_with_category()
    {
        var searchResults = new PagedResult<BookSearchResultDto>(
        [
            new BookSearchResultDto(
                Guid.Parse("90909090-9090-9090-9090-909090909090"),
                "Axborot xavfsizligi",
                "A. Muallif",
                "Xavfsizlik",
                null,
                AverageRating: 4.8m,
                RatingsCount: 12,
                Status: "Verified"),
        ], Page: 2, PageSize: 9, TotalCount: 12);
        var searchService = new FakeBookSearchService(searchResults);
        var controller = new BooksController(new FakeBookService(), searchService);

        var response = await controller.SearchBooks(
            q: " xavfsizlik ",
            page: 2,
            pageSize: 9,
            category: "Xavfsizlik",
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(searchResults, okResult.Value);
        Assert.Equal("xavfsizlik", searchService.LastQuery);
        Assert.Equal(2, searchService.LastPagination?.Page);
        Assert.Equal(9, searchService.LastPagination?.PageSize);
        Assert.Equal("Xavfsizlik", searchService.LastCategory);
    }

    private sealed class FakeBookSearchService(PagedResult<BookSearchResultDto> searchResults) : IBookSearchService
    {
        public string? LastQuery { get; private set; }

        public PaginationQuery? LastPagination { get; private set; }

        public string? LastCategory { get; private set; }

        public Task<PagedResult<BookSearchResultDto>> SearchAsync(
            string query,
            PaginationQuery pagination,
            string? category,
            CancellationToken cancellationToken)
        {
            LastQuery = query;
            LastPagination = pagination;
            LastCategory = category;
            return Task.FromResult(searchResults);
        }
    }

    private sealed class FakeBookService : IBookService
    {
        public Task<PagedResult<BookListItemDto>> GetBooksAsync(
            PaginationQuery pagination,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<PagedResult<BookListItemDto>> GetBooksAsync(
            PaginationQuery pagination,
            string? category,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<string>> GetBookCategoriesAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<BookListItemDto>> GetTopRatedBooksAsync(int limit, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<BookDetailsDto> GetBookByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<BookDetailsDto> CreateBookAsync(CreateBookCommand command, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<BookDetailsDto> UpdateBookAsync(Guid id, UpdateBookCommand command, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task DeleteBookAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<BookDetailsDto> SubmitRatingAsync(
            Guid bookId,
            SubmitBookRatingCommand command,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
