using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Auth;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_creates_non_admin_user_with_hashed_password()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var authService = new AuthService(
            repository,
            new FakePasswordHashService(),
            new FakeJwtTokenService(),
            unitOfWork);

        var result = await authService.RegisterAsync(
            new RegisterUserCommand(" user1 ", "user1@example.com", "Password123!"),
            CancellationToken.None);

        var user = Assert.Single(repository.Users);
        Assert.Equal("user1", user.Username);
        Assert.Equal("user1@example.com", user.Email);
        Assert.Equal("hashed:Password123!", user.PasswordHash);
        Assert.Null(user.ProfilePictureUrl);
        Assert.False(user.IsAdmin);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Null(result.User.ProfilePictureUrl);
        Assert.Equal("token:user1:False", result.Token);
    }

    [Fact]
    public void User_create_keeps_optional_profile_picture_url()
    {
        var user = User.Create(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            "user2",
            "user2@example.com",
            "hashed:Password123!",
            profilePictureUrl: " https://example.com/faces/user2.jpg ",
            isAdmin: false,
            createdAt: new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal("https://example.com/faces/user2.jpg", user.ProfilePictureUrl);
    }

    [Fact]
    public async Task LoginAsync_returns_token_for_valid_credentials_and_preserves_admin_flag()
    {
        var admin = User.Create(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "admin",
            "admin@bookrate.uz",
            "hashed:Admin123!",
            profilePictureUrl: "https://example.com/faces/admin.jpg",
            isAdmin: true,
            createdAt: new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero));
        var authService = new AuthService(
            new FakeUserRepository(admin),
            new FakePasswordHashService(),
            new FakeJwtTokenService(),
            new FakeUnitOfWork());

        var result = await authService.LoginAsync(
            new LoginCommand("admin", "Admin123!"),
            CancellationToken.None);

        Assert.Equal(admin.Id, result.User.Id);
        Assert.Equal("https://example.com/faces/admin.jpg", result.User.ProfilePictureUrl);
        Assert.True(result.User.IsAdmin);
        Assert.Equal("token:admin:True", result.Token);
    }

    [Fact]
    public async Task LoginAsync_rejects_invalid_password()
    {
        var user = User.Create(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "user1",
            "user1@example.com",
            "hashed:Password123!",
            profilePictureUrl: null,
            isAdmin: false,
            createdAt: new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero));
        var authService = new AuthService(
            new FakeUserRepository(user),
            new FakePasswordHashService(),
            new FakeJwtTokenService(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<InvalidLoginException>(() =>
            authService.LoginAsync(new LoginCommand("user1", "wrong"), CancellationToken.None));
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public List<User> Users { get; } = [.. users];

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            var user = Users.SingleOrDefault(candidate =>
                string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(user);
        }

        public Task<bool> ExistsByUsernameOrEmailAsync(
            string username,
            string email,
            CancellationToken cancellationToken)
        {
            var exists = Users.Any(user =>
                string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)
                || string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }

        public void Add(User user)
        {
            Users.Add(user);
        }
    }

    private sealed class FakePasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return $"hashed:{password}";
        }

        public bool VerifyPassword(string passwordHash, string password)
        {
            return passwordHash == $"hashed:{password}";
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public string CreateToken(User user)
        {
            return $"token:{user.Username}:{user.IsAdmin}";
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }
}
