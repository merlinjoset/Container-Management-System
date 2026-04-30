using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Auth;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class EfUserManagementRepository : IUserManagementRepository
{
    private readonly AppDbContext _db;
    public EfUserManagementRepository(AppDbContext db) => _db = db;

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
        => _db.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);

    public async Task<Guid> CreateUserAsync(string? userName, string email, string passwordHash, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        _db.Users.Add(new UserEntity
        {
            Id = id,
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            PasswordHash = passwordHash,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0
        });
        return id;
    }

    public async Task EnsureRoleExistsAsync(string roleName, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role == null)
        {
            _db.Roles.Add(new RoleEntity { Id = Guid.NewGuid(), Name = roleName });
        }
    }

    public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct)
                   ?? throw new InvalidOperationException("Role missing.");

        var exists = await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, ct);
        if (!exists)
        {
            _db.UserRoles.Add(new UserRoleEntity { UserId = userId, RoleId = role.Id });
        }
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Roles = new List<string>()
            })
            .ToListAsync(ct);

        var roleMap = await _db.UserRoles
            .AsNoTracking()
            .Select(ur => new { ur.UserId, RoleName = ur.Role.Name })
            .ToListAsync(ct);

        var dict = roleMap.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Where(x => !string.IsNullOrWhiteSpace(x.RoleName))
                                            .Select(x => x.RoleName!)
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .OrderBy(r => r)
                                            .ToList());

        foreach (var u in users)
        {
            if (dict.TryGetValue(u.Id, out var roles))
                u.Roles = roles;
        }

        return users.OrderBy(u => u.Email ?? u.UserName).ToList();
    }

    public async Task<List<RoleListItemDto>> GetRolesAsync(CancellationToken ct)
    {
        var roles = await _db.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleListItemDto { Id = r.Id, Name = r.Name ?? "" })
            .ToListAsync(ct);

        // Fetch all user-role pairs joined with user info (username/email)
        var assignments = await _db.UserRoles.AsNoTracking()
            .Select(ur => new
            {
                ur.RoleId,
                Username = ur.User != null ? (ur.User.UserName ?? ur.User.Email ?? "") : ""
            })
            .ToListAsync(ct);

        var byRole = assignments
            .Where(a => !string.IsNullOrWhiteSpace(a.Username))
            .GroupBy(a => a.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Username!).OrderBy(u => u, StringComparer.OrdinalIgnoreCase).ToList());

        foreach (var r in roles)
        {
            if (byRole.TryGetValue(r.Id, out var names))
            {
                r.UserNames = names;
                r.UserCount = names.Count;
            }
        }
        return roles;
    }
}
