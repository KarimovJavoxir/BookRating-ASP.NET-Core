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

    [HttpPost]
    [ProducesResponseType(typeof(BookDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDetailsDto>> CreateBook(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.CreateBookAsync(
                new CreateBookCommand(
                    request.Title!,
                    request.Author!,
                    request.Category,
                    request.Description,
                    request.PublishedYear,
                    request.CoverImageUrl),
                cancellationToken);

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }
        catch (ArgumentException exception)
        {
            return InvalidBookRequest(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetailsDto>> UpdateBook(
        Guid id,
        UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var book = await bookService.UpdateBookAsync(
                id,
                new UpdateBookCommand(
                    request.Title!,
                    request.Author!,
                    request.Category,
                    request.Description,
                    request.PublishedYear,
                    request.CoverImageUrl),
                cancellationToken);

            return Ok(book);
        }
        catch (BookNotFoundException exception)
        {
            return BookNotFound(exception.BookId);
        }
        catch (ArgumentException exception)
        {
            return InvalidBookRequest(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await bookService.DeleteBookAsync(id, cancellationToken);
            return NoContent();
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

        try
        {
            var books = await bookSearchService.SearchAsync(q, cancellationToken);
            return Ok(books);
        }
        catch (BookSearchUnavailableException exception)
        {
            return Problem(
                title: "Qidiruv servisi vaqtincha ishlamayapti.",
                detail: exception.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
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

    private ActionResult InvalidBookRequest(ArgumentException exception)
    {
        return Problem(
            title: "Kitob maʼlumotlari notoʻgʻri.",
            detail: exception.Message,
            statusCode: StatusCodes.Status400BadRequest);
    }
}
