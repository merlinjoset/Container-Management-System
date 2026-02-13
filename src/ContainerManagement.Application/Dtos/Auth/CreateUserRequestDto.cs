using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Auth;

public class CreateUserRequestDto
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string? UserName { get; set; }

    [Required, MinLength(6), MaxLength(100)]
    public string? Password { get; set; }

    public bool IsAdmin { get; set; } = false;
}

