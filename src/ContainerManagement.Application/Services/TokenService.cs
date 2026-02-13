using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Auth;
using ContainerManagement.Application.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ContainerManagement.Application.Services;

public class TokenService : ITokenService
{
    private readonly IAuthUserRepository _users;
    private readonly IConfiguration _cfg;
    private readonly ITokenStore _tokenStore;

    public TokenService(IAuthUserRepository users, IConfiguration cfg, ITokenStore tokenStore)
    {
        _users = users;
        _cfg = cfg;
        _tokenStore = tokenStore;
    }

    public async Task<LoginResponseDto> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var u = await _users.GetByEmailAsync(email, ct);
        if (u == null || string.IsNullOrWhiteSpace(u.Value.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!PasswordHasher.Verify(password, u.Value.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _users.GetRolesAsync(u.Value.Id, ct);

        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;
        var key = _cfg["Jwt:Key"]!;
        var expiresMin = int.Parse(_cfg["Jwt:ExpiresMinutes"] ?? "60");
        var jti = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, u.Value.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, u.Value.Id.ToString()),
            new Claim(ClaimTypes.Email, u.Value.Email ?? ""),
            new Claim("username", u.Value.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
        };

        foreach (var r in roles.Where(x => !string.IsNullOrWhiteSpace(x))
                          .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var v = r.Trim();
            claims.Add(new Claim(ClaimTypes.Role, v));
            claims.Add(new Claim("role", v));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMin);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: creds);

        await _tokenStore.SaveJtiAsync(u.Value.Id, jti, expiresAtUtc, ct);

        return new LoginResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
