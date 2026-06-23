using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Repositories;

internal sealed class EfUserRepository(BookRatingDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Username.ToLower() == username.ToLower(),
                cancellationToken);
    }

    public Task<bool> ExistsByUsernameOrEmailAsync(
        string username,
        string email,
        CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(
            user => user.Username.ToLower() == username.ToLower()
                || user.Email.ToLower() == email.ToLower(),
            cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }
}
