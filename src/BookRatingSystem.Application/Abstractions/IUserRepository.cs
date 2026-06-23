using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken);

    void Add(User user);
}
