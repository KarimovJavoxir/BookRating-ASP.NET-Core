using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class BooksController(
    IBookService bookService,
    IBookSearchService bookSearchService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookListItemDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await bookService.GetBooksAsync(cancellationToken);
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetailsDto>> GetBook(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.GetBookByIdAsync(id, cancellationToken);
            return Ok(book);
        }
        catch (BookNotFoundException exception)
        {
            return BookNotFound(exception.BookId);
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<BookSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BookSearchResultDto>>> SearchBooks(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Problem(
                title: "Qidiruv soʻrovi kiritilmagan.",
                detail: "q query parametri boʻsh boʻlmasligi kerak.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var books = await bookSearchService.SearchAsync(q, cancellationToken);
        return Ok(books);
    }

    [HttpPost("{id:guid}/ratings")]
    [ProducesResponseType(typeof(BookDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetailsDto>> SubmitRating(
        Guid id,
        CreateBookRatingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.SubmitRatingAsync(
                id,
                new SubmitBookRatingCommand(request.Value, request.Comment),
                cancellationToken);

            return Ok(book);
        }
        catch (BookNotFoundException exception)
        {
            return BookNotFound(exception.BookId);
        }
        catch (InvalidBookRatingException exception)
        {
            return Problem(
                title: "Reyting qiymati notoʻgʻri.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private ActionResult BookNotFound(Guid bookId)
    {
        return Problem(
            title: "Kitob topilmadi.",
            detail: $"Id qiymati '{bookId}' boʻlgan kitob topilmadi.",
            statusCode: StatusCodes.Status404NotFound);
    }
}
