namespace BookRatingSystem.Application.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken);

    Task<AuthResultDto> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
}
