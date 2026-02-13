using ContainerManagement.Application.Abstractions;
using ContainerManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly AppDbContext _db;
    public AuthUserRepository(AppDbContext db) => _db = db;

    public async Task<(Guid Id, string? Email, string? UserName, string? PasswordHash)?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var e = email.Trim();
        return await _db.Users
            .Where(u => u.Email == e)
            .Select(u => new ValueTuple<Guid, string?, string?, string?>(u.Id, u.Email, u.UserName, u.PasswordHash))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);
    }
}
