using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
