namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class UserEntity
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
    public ICollection<UserLoginEntity> UserLogins { get; set; } = new List<UserLoginEntity>();
    public ICollection<UserTokenEntity> UserTokens { get; set; } = new List<UserTokenEntity>();
}
