namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; } 

    public UserEntity? User { get; set; }
    public RoleEntity? Role { get; set; }
}
