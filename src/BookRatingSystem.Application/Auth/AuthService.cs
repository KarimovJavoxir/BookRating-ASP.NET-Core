using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var username = Normalize(command.Username, nameof(command.Username));
        var email = Normalize(command.Email, nameof(command.Email)).ToLowerInvariant();
        var password = Normalize(command.Password, nameof(command.Password));

        if (await userRepository.ExistsByUsernameOrEmailAsync(username, email, cancellationToken))
        {
            throw new DuplicateUserException();
        }

        var user = User.Create(
            Guid.NewGuid(),
            username,
            email,
            passwordHashService.HashPassword(password),
            isAdmin: false,
            DateTimeOffset.UtcNow);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAuthResult(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var username = Normalize(command.Username, nameof(command.Username));
        var password = Normalize(command.Password, nameof(command.Password));

        var user = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !passwordHashService.VerifyPassword(user.PasswordHash, password))
        {
            throw new InvalidLoginException();
        }

        return CreateAuthResult(user);
    }

    private AuthResultDto CreateAuthResult(User user)
    {
        return new AuthResultDto(
            jwtTokenService.CreateToken(user),
            new AuthUserDto(user.Id, user.Username, user.Email, user.IsAdmin));
    }

    private static string Normalize(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
