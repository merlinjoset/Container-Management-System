namespace ContainerManagement.Application.Abstractions;

public interface IAuthUserRepository
{
    Task<(Guid Id, string? Email, string? UserName, string? PasswordHash)?> GetByEmailAsync(string email, CancellationToken ct);
    Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct);
}
