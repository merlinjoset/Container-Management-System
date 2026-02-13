namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class UserTokenEntity
{
    public Guid UserId { get; set; }
    public string? LoginProvider { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }

    public UserEntity? User { get; set; }
}
