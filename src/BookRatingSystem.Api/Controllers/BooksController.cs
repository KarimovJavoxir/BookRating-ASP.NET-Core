using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
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
                    request.CoverImageUrl,
                    ParseBookStatus(request.Status)),
                cancellationToken);

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }
        catch (ArgumentException exception)
        {
            return InvalidBookRequest(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
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
                    request.CoverImageUrl,
                    ParseBookStatus(request.Status)),
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
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
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
    [Authorize(Policy = AuthorizationPolicies.AuthenticatedUser)]
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
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Problem(
                    title: "Foydalanuvchi aniqlanmadi.",
                    detail: "JWT token ichida foydalanuvchi identifikatori topilmadi.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var book = await bookService.SubmitRatingAsync(
                id,
                new SubmitBookRatingCommand(userId.Value, request.Value, request.Comment),
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

    private static BookStatus ParseBookStatus(string? status)
    {
        if (Enum.TryParse<BookStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            return parsedStatus;
        }

        throw new ArgumentException("Book status must be New, Banned, or Verified.", nameof(status));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
