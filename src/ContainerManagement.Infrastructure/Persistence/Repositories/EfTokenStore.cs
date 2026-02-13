using ContainerManagement.Application.Abstractions;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class EfTokenStore : ITokenStore
{
    private readonly AppDbContext _db;
    public EfTokenStore(AppDbContext db) => _db = db;

    private const string Provider = "JWT";
    private const string Name = "jti";

    public async Task SaveJtiAsync(Guid userId, string jti, DateTime expiresAtUtc, CancellationToken ct)
    {
        // Store Value as "jti|expiresTicks" (since your schema has only Value)
        var value = $"{jti}|{expiresAtUtc.Ticks}";

        var existing = await _db.UserTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LoginProvider == Provider && x.Name == Name, ct);

        if (existing == null)
        {
            _db.UserTokens.Add(new UserTokenEntity
            {
                UserId = userId,
                LoginProvider = Provider,
                Name = Name,
                Value = value
            });
        }
        else
        {
            existing.Value = value;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsJtiActiveAsync(Guid userId, string jti, CancellationToken ct)
    {
        var row = await _db.UserTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LoginProvider == Provider && x.Name == Name, ct);

        if (row?.Value == null) return false;

        var parts = row.Value.Split('|');
        if (parts.Length != 2) return false;

        var storedJti = parts[0];
        if (!long.TryParse(parts[1], out var ticks)) return false;

        var expiresUtc = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow >= expiresUtc) return false;

        return string.Equals(storedJti, jti, StringComparison.Ordinal);
    }

    public async Task RevokeJtiAsync(Guid userId, string jti, CancellationToken ct)
    {
        var row = await _db.UserTokens
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LoginProvider == Provider && x.Name == Name, ct);

        if (row == null) return;

        // revoke by clearing (or delete row)
        _db.UserTokens.Remove(row);
        await _db.SaveChangesAsync(ct);
    }
}
