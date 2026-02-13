namespace ContainerManagement.Application.Abstractions;

public interface IUserManagementRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<Guid> CreateUserAsync(string? userName, string email, string passwordHash, CancellationToken ct);

    Task EnsureRoleExistsAsync(string roleName, CancellationToken ct);
    Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct);

    Task<List<ContainerManagement.Application.Dtos.Auth.UserListItemDto>> GetUsersAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
