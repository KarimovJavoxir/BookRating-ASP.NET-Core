using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResultDto>> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RegisterAsync(
                new RegisterUserCommand(request.Username!, request.Email!, request.Password!),
                cancellationToken);

            return CreatedAtAction(nameof(Register), result);
        }
        catch (DuplicateUserException exception)
        {
            return Problem(
                title: "Foydalanuvchi mavjud.",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                title: "Roʻyxatdan oʻtish maʼlumotlari notoʻgʻri.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResultDto>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.LoginAsync(
                new LoginCommand(request.Username!, request.Password!),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidLoginException exception)
        {
            return Problem(
                title: "Login yoki parol notoʻgʻri.",
                detail: exception.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }
}
