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
    public DbSet<RegionEntity> Regions => Set<RegionEntity>();
    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<TerminalEntity> Terminals => Set<TerminalEntity>();
    public DbSet<VesselEntity> Vessels => Set<VesselEntity>();
    public DbSet<VendorEntity> Vendors => Set<VendorEntity>();
    public DbSet<OperatorEntity> Operators => Set<OperatorEntity>();

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

        // ---------------- TblRegions ----------------
        b.Entity<RegionEntity>(e =>
        {
            e.ToTable("TblRegion");

            e.HasKey(x => x.Id);

            e.Property(x => x.RegionName).HasMaxLength(100);

            e.Property(x => x.RegionCode).HasMaxLength(20);

            e.Property(x => x.IsDeleted).IsRequired();

            e.Property(x => x.CreatedOn).IsRequired();

            e.Property(x => x.ModifiedOn).IsRequired();

            e.Property(x => x.CreatedBy).IsRequired();

            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblCountry ----------------
        b.Entity<CountryEntity>(e =>
        {
            e.ToTable("TblCountry");

            e.HasKey(x => x.Id);

            e.Property(x => x.CountryName).HasMaxLength(100);

            e.Property(x => x.CountryCode).HasMaxLength(20);

            e.Property(x => x.IsDeleted).IsRequired();

            e.Property(x => x.CreatedOn).IsRequired();

            e.Property(x => x.ModifiedOn).IsRequired();

            e.Property(x => x.CreatedBy).IsRequired();

            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblTerminal ----------------
        b.Entity<TerminalEntity>(e =>
        {
            e.ToTable("TblTerminal");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasDefaultValueSql("NEWID()");

            e.Property(x => x.PortId).IsRequired();

            e.Property(x => x.TerminalName)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.TerminalCode)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            e.Property(x => x.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            e.Property(x => x.ModifiedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.PortId)
                .HasConstraintName("FK_Terminal_Port")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.PortId)
                .HasDatabaseName("IX_Terminal_PortId");
        });

        // ---------------- TblVessel ----------------
        b.Entity<VesselEntity>(e =>
        {
            e.ToTable("TblVessel");

            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("NEWID()");

            e.Property(x => x.VesselName).HasMaxLength(150).IsRequired();
            e.Property(x => x.VesselCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ImoCode).HasMaxLength(20).IsRequired();

            e.Property(x => x.Flag).HasMaxLength(100);

            e.Property(x => x.NRT).HasColumnType("decimal(18,2)");
            e.Property(x => x.GRT).HasColumnType("decimal(18,2)");
            e.Property(x => x.Speed).HasColumnType("decimal(5,2)");

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).HasDefaultValueSql("GETUTCDATE()").IsRequired();
            e.Property(x => x.ModifiedOn).HasDefaultValueSql("GETUTCDATE()").IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblVendor ----------------
        b.Entity<VendorEntity>(e =>
        {
            e.ToTable("TblVendor");

            e.HasKey(x => x.Id);

            e.Property(x => x.VendorName).HasMaxLength(150).IsRequired();
            e.Property(x => x.VendorCode).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.VendorCode).IsUnique().HasDatabaseName("UQ_TblVendor_VendorCode");

            e.Property(x => x.CountryId).IsRequired();

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<CountryEntity>()
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ---------------- TblOperator ----------------
        b.Entity<OperatorEntity>(e =>
        {
            e.ToTable("TblOperator");

            e.HasKey(x => x.Id);

            e.Property(x => x.OperatorName)
                .HasMaxLength(150)
                .IsRequired()
                .HasColumnName("Operator");

            // UniqueCode removed per latest requirements

            e.Property(x => x.VendorId).IsRequired();
            e.Property(x => x.CountryId).IsRequired();

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<VendorEntity>()
                .WithMany()
                .HasForeignKey(x => x.VendorId)
                .HasConstraintName("FK_TblOperator_TblVendor")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<CountryEntity>()
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .HasConstraintName("FK_TblOperator_TblCountry")
                .OnDelete(DeleteBehavior.NoAction);
        });

    }
}
