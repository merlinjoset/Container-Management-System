using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<UserLoginEntity> UserLogins => Set<UserLoginEntity>();
    public DbSet<UserTokenEntity> UserTokens => Set<UserTokenEntity>();
    public DbSet<PortsEntity> Ports => Set<PortsEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---------------- Users ----------------
        b.Entity<UserEntity>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);

            e.Property(x => x.UserName).HasMaxLength(256);
            e.Property(x => x.Email).HasMaxLength(256);

            e.Property(x => x.EmailConfirmed).IsRequired();
            e.Property(x => x.PhoneNumberConfirmed).IsRequired();
            e.Property(x => x.TwoFactorEnabled).IsRequired();
            e.Property(x => x.LockoutEnabled).IsRequired();
            e.Property(x => x.AccessFailedCount).IsRequired();

            e.Property(x => x.LockoutEnd).HasColumnType("datetimeoffset");
        });

        // ---------------- Roles ----------------
        b.Entity<RoleEntity>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // ---------------- UserRoles (UserId + RoleId) ----------------
        b.Entity<UserRoleEntity>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(x => new { x.UserId, x.RoleId });
        });

        // ---------------- UserLogins (LoginProvider + ProviderKey) ----------------
        b.Entity<UserLoginEntity>(e =>
        {
            e.ToTable("UserLogins");
            e.HasKey(x => new { x.LoginProvider, x.ProviderKey });

            e.Property(x => x.LoginProvider).HasMaxLength(128).IsRequired();
            e.Property(x => x.ProviderKey).HasMaxLength(128).IsRequired();

        });

        // ---------------- UserTokens (UserId + LoginProvider + Name) ----------------
        b.Entity<UserTokenEntity>(e =>
        {
            e.ToTable("UserTokens");
            e.HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            e.Property(x => x.LoginProvider).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
        });

        // ---------------- TblPorts ----------------
        b.Entity<PortsEntity>(e =>
        {
            e.ToTable("TblPorts");

            e.HasKey(x => x.Id);

            e.Property(x => x.PortCode).HasMaxLength(20);

            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();

            e.Property(x => x.Country).HasMaxLength(100).IsRequired();

            e.Property(x => x.Region).HasMaxLength(100);

            e.Property(x => x.RegionCode).HasMaxLength(20);

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();

            e.Property(x => x.CreatedOn).IsRequired();

            e.Property(x => x.ModifiedOn).IsRequired();

            e.Property(x => x.CreatedBy).IsRequired();

            e.Property(x => x.ModifiedBy).IsRequired();
        });

    }
}
