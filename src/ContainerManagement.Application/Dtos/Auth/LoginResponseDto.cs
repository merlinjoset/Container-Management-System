namespace ContainerManagement.Application.Dtos.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}
