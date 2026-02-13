namespace ContainerManagement.Application.Dtos.Auth;

public class UserListItemDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public List<string> Roles { get; set; } = new();
}

