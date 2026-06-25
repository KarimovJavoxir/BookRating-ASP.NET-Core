using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Api.Controllers;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [Fact]
    public async Task SubmitRating_returns_clear_error_when_user_already_rated_book()
    {
        var bookId = Guid.Parse("91919191-9191-9191-9191-919191919191");
        var userId = Guid.Parse("10000000-0000-0000-0000-000000000091");
        var bookService = new FakeBookService
        {
            SubmitRatingException = new InvalidBookRatingException("Siz bu kitobga allaqachon baho qoldirgansiz."),
        };
        var controller = new BooksController(bookService, new FakeBookSearchService(new PagedResult<BookSearchResultDto>([], 1, 10, 0)))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    ], authenticationType: "Test")),
                },
            },
        };

        var response = await controller.SubmitRating(
            bookId,
            new CreateBookRatingRequest(5, "Ikkinchi izoh"),
            CancellationToken.None);

        var problemResult = Assert.IsType<ObjectResult>(response.Result);
        var problem = Assert.IsType<ProblemDetails>(problemResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemResult.StatusCode);
        Assert.Equal("Reyting yuborilmadi.", problem.Title);
        Assert.Equal("Siz bu kitobga allaqachon baho qoldirgansiz.", problem.Detail);
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
        public InvalidBookRatingException? SubmitRatingException { get; init; }

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
            CancellationToken cancellationToken)
        {
            if (SubmitRatingException is not null)
            {
                throw SubmitRatingException;
            }

            throw new NotSupportedException();
        }
    }
}
