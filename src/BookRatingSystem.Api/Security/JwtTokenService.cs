using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookRatingSystem.Api.Security;

internal sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public string CreateToken(User user)
    {
        var jwtOptions = options.Value;
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new("is_admin", user.IsAdmin ? "true" : "false")
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiresInMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
