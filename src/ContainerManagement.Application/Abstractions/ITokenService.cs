using ContainerManagement.Application.Dtos.Auth;

namespace ContainerManagement.Application.Abstractions;

public interface ITokenService
{
    Task<LoginResponseDto> LoginAsync(string email, string password, CancellationToken ct = default);
}
