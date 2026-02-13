using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Auth;
using ContainerManagement.Application.Security;

namespace ContainerManagement.Application.Services;

public class UserAdminService
{
    private readonly IUserManagementRepository _repo;

    public UserAdminService(IUserManagementRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateUserAsync(CreateUserRequestDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim();
        var username = dto.UserName.Trim();

        if (await _repo.EmailExistsAsync(email, ct))
            throw new InvalidOperationException("Email already exists.");

        var pwdHash = PasswordHasher.Hash(dto.Password);
        var id = await _repo.CreateUserAsync(username, email, pwdHash, ct);

        // Ensure roles and assign
        await _repo.EnsureRoleExistsAsync(AppRoles.User, ct);
        await _repo.AssignRoleAsync(id, AppRoles.User, ct);

        if (dto.IsAdmin)
        {
            await _repo.EnsureRoleExistsAsync(AppRoles.Admin, ct);
            await _repo.AssignRoleAsync(id, AppRoles.Admin, ct);
        }

        await _repo.SaveChangesAsync(ct);
        return id;
    }

    public Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct)
        => _repo.GetUsersAsync(ct);
}
