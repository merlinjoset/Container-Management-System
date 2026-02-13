namespace ContainerManagement.Application.Abstractions;

public interface ITokenStore
{
    Task SaveJtiAsync(Guid userId, string jti, DateTime expiresAtUtc, CancellationToken ct);
    Task<bool> IsJtiActiveAsync(Guid userId, string jti, CancellationToken ct);
    Task RevokeJtiAsync(Guid userId, string jti, CancellationToken ct);
}
