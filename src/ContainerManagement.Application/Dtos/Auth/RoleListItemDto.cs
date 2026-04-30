namespace ContainerManagement.Application.Dtos.Auth;

public class RoleListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public List<string> UserNames { get; set; } = new();
}
