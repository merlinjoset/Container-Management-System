using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Auth;

public class LoginRequestDto
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required] public string Password { get; set; } = default!;
}
