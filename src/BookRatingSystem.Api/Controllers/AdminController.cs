using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Application.Admin;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AdminController(
    IAdminReadService adminReadService,
    IAdminRatingModerationService adminRatingModerationService) : ControllerBase
{
    [HttpGet("books")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(PagedResult<AdminBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminBookDto>>> GetBooks(
        [FromQuery] int page = PaginationQuery.DefaultPage,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var books = await adminReadService.GetBooksAsync(new PaginationQuery(page, pageSize), cancellationToken);
        return Ok(books);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var dashboard = await adminReadService.GetDashboardAsync(from, to, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("users")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetUsers(
        [FromQuery] int page = PaginationQuery.DefaultPage,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var users = await adminReadService.GetUsersAsync(new PaginationQuery(page, pageSize), cancellationToken);
        return Ok(users);
    }

    [HttpGet("ratings")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(PagedResult<AdminBookRatingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminBookRatingDto>>> GetRatings(
        [FromQuery] int page = PaginationQuery.DefaultPage,
        [FromQuery] int pageSize = PaginationQuery.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var ratings = await adminReadService.GetRatingsAsync(new PaginationQuery(page, pageSize), cancellationToken);
        return Ok(ratings);
    }

    [HttpPost("ratings/{ratingId:guid}/accept")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptRating(Guid ratingId, CancellationToken cancellationToken)
    {
        try
        {
            await adminRatingModerationService.AcceptRatingAsync(ratingId, cancellationToken);
            return NoContent();
        }
        catch (BookRatingNotFoundException exception)
        {
            return RatingNotFound(exception.RatingId);
        }
    }

    [HttpPost("ratings/{ratingId:guid}/ban")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BanRating(
        Guid ratingId,
        BanBookRatingRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            await adminRatingModerationService.BanRatingAsync(ratingId, request?.BanReason, cancellationToken);
            return NoContent();
        }
        catch (BookRatingNotFoundException exception)
        {
            return RatingNotFound(exception.RatingId);
        }
        catch (InvalidBookRatingException exception)
        {
            return Problem(
                title: "Reyting moderatsiyasi bajarilmadi.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private ActionResult RatingNotFound(Guid ratingId)
    {
        return Problem(
            title: "Reyting topilmadi.",
            detail: $"Id qiymati '{ratingId}' boʻlgan reyting topilmadi.",
            statusCode: StatusCodes.Status404NotFound);
    }
}
