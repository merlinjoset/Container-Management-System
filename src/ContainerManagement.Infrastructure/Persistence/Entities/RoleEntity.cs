namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class RoleEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
