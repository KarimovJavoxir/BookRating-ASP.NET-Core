using BookRatingSystem.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AdminController(IAdminReadService adminReadService) : ControllerBase
{
    [HttpGet("books")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(IReadOnlyList<AdminBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminBookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await adminReadService.GetBooksAsync(cancellationToken);
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
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await adminReadService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("ratings")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(IReadOnlyList<AdminBookRatingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminBookRatingDto>>> GetRatings(CancellationToken cancellationToken)
    {
        var ratings = await adminReadService.GetRatingsAsync(cancellationToken);
        return Ok(ratings);
    }
}
