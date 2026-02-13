namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class UserLoginEntity
{
    public string LoginProvider { get; set; } = default!;
    public string ProviderKey { get; set; } = default!;
    public string? ProviderDisplayName { get; set; }

    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
}
