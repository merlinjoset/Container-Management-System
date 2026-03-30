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
    public DbSet<ServiceEntity> Services => Set<ServiceEntity>();
    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<DistanceEntity> Distances => Set<DistanceEntity>();
    public DbSet<SlotEntity> Slots => Set<SlotEntity>();
    public DbSet<VoyageEntity> Voyages => Set<VoyageEntity>();
    public DbSet<VoyagePortEntity> VoyagePorts => Set<VoyagePortEntity>();
    public DbSet<VoyagePortArrivalEntity> VoyagePortArrivals => Set<VoyagePortArrivalEntity>();
    public DbSet<VoyagePortDepartureEntity> VoyagePortDepartures => Set<VoyagePortDepartureEntity>();
    public DbSet<JobEntity> Jobs => Set<JobEntity>();
    public DbSet<JobAttachmentEntity> JobAttachments => Set<JobAttachmentEntity>();

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

            e.Property(x => x.CountryId).IsRequired();
            e.Property(x => x.RegionId).IsRequired();

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<CountryEntity>()
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .HasConstraintName("FK_TblPorts_TblCountry")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<RegionEntity>()
                .WithMany()
                .HasForeignKey(x => x.RegionId)
                .HasConstraintName("FK_TblPorts_TblRegion")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.CountryId).HasDatabaseName("IX_TblPorts_CountryId");
            e.HasIndex(x => x.RegionId).HasDatabaseName("IX_TblPorts_RegionId");
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
            e.Property(x => x.IsCompetitor).IsRequired().HasDefaultValue(false);

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
        });

        // ---------------- TblService ----------------
        b.Entity<ServiceEntity>(e =>
        {
            e.ToTable("TblService");

            e.HasKey(x => x.Id);

            e.Property(x => x.ServiceCode).HasMaxLength(50);
            e.HasIndex(x => x.ServiceCode).IsUnique().HasDatabaseName("UQ_TblService_ServiceCode");

            e.Property(x => x.ServiceName).HasMaxLength(150);

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblRoute ----------------
        b.Entity<RouteEntity>(e =>
        {
            e.ToTable("TblRoute");

            e.HasKey(x => x.Id);

            e.Property(x => x.RouteName).HasMaxLength(150);
            e.HasIndex(x => x.RouteName).IsUnique().HasDatabaseName("UQ_TblRoute_RouteName");

            e.Property(x => x.PortOfOriginId).IsRequired();
            e.Property(x => x.FinalDestinationId).IsRequired();

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.PortOfOriginId)
                .HasConstraintName("FK_TblRoute_TblPorts_Origin")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.FinalDestinationId)
                .HasConstraintName("FK_TblRoute_TblPorts_Destination")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.PortOfOriginId).HasDatabaseName("IX_TblRoute_PortOfOriginId");
            e.HasIndex(x => x.FinalDestinationId).HasDatabaseName("IX_TblRoute_FinalDestinationId");
        });

        // ---------------- TblPortDistance ----------------
        b.Entity<DistanceEntity>(e =>
        {
            e.ToTable("TblPortDistance");

            e.HasKey(x => x.Id);

            e.Property(x => x.FromPortId).IsRequired();
            e.Property(x => x.ToPortId).IsRequired();
            e.Property(x => x.Distance).HasColumnType("decimal(10,2)").IsRequired();

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasIndex(x => new { x.FromPortId, x.ToPortId })
                .IsUnique()
                .HasDatabaseName("UQ_TblDistance_FromTo");

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.FromPortId)
                .HasConstraintName("FK_TblDistance_TblPorts_From")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.ToPortId)
                .HasConstraintName("FK_TblDistance_TblPorts_To")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.FromPortId).HasDatabaseName("IX_TblDistance_FromPortId");
            e.HasIndex(x => x.ToPortId).HasDatabaseName("IX_TblDistance_ToPortId");
        });

        // ---------------- TblSlot ----------------
        b.Entity<SlotEntity>(e =>
        {
            e.ToTable("TblSlot");

            e.HasKey(x => x.Id);

            e.Property(x => x.SlotName).HasMaxLength(150);
            e.HasIndex(x => x.SlotName).IsUnique().HasDatabaseName("UQ_TblSlot_SlotName");

            e.Property(x => x.IsDeleted).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblVoyage ----------------
        b.Entity<VoyageEntity>(e =>
        {
            e.ToTable("TblVoyage");
            e.HasKey(x => x.Id);

            e.Property(x => x.VesselId).IsRequired();
            e.Property(x => x.VoyageType).HasMaxLength(50);

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<VesselEntity>()
                .WithMany()
                .HasForeignKey(x => x.VesselId)
                .HasConstraintName("FK_TblVoyage_TblVessel")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<ServiceEntity>()
                .WithMany()
                .HasForeignKey(x => x.ServiceId)
                .HasConstraintName("FK_TblVoyage_TblService")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<OperatorEntity>()
                .WithMany()
                .HasForeignKey(x => x.OperatorId)
                .HasConstraintName("FK_TblVoyage_TblOperator")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.VesselId).HasDatabaseName("IX_TblVoyage_VesselId");
            e.HasIndex(x => x.ServiceId).HasDatabaseName("IX_TblVoyage_ServiceId");
        });

        // ---------------- TblVoyagePort ----------------
        b.Entity<VoyagePortEntity>(e =>
        {
            e.ToTable("TblVoyagePort");
            e.HasKey(x => x.Id);

            e.Property(x => x.VoyageId).IsRequired();
            e.Property(x => x.VoyNo).HasMaxLength(50);
            e.Property(x => x.Bound).HasMaxLength(20);
            e.Property(x => x.PortId).IsRequired();
            e.Property(x => x.SeaDay).HasColumnType("decimal(10,2)");
            e.Property(x => x.Speed).HasColumnType("decimal(10,2)");
            e.Property(x => x.Distance).HasColumnType("decimal(10,2)");

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<VoyageEntity>()
                .WithMany()
                .HasForeignKey(x => x.VoyageId)
                .HasConstraintName("FK_TblVoyagePort_TblVoyage")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.PortId)
                .HasConstraintName("FK_TblVoyagePort_TblPorts")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<TerminalEntity>()
                .WithMany()
                .HasForeignKey(x => x.TerminalId)
                .HasConstraintName("FK_TblVoyagePort_TblTerminal")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.VoyageId).HasDatabaseName("IX_TblVoyagePort_VoyageId");
            e.HasIndex(x => new { x.VoyageId, x.SortOrder }).HasDatabaseName("IX_TblVoyagePort_VoyageId_SortOrder");
        });

        // ---------------- TblJob ----------------
        b.Entity<JobEntity>(e =>
        {
            e.ToTable("TblJob");
            e.HasKey(x => x.Id);

            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.Tag).HasMaxLength(100);
            e.Property(x => x.TagColor).HasMaxLength(30);

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();
        });

        // ---------------- TblJobAttachment ----------------
        b.Entity<JobAttachmentEntity>(e =>
        {
            e.ToTable("TblJobAttachment");
            e.HasKey(x => x.Id);

            e.Property(x => x.JobId).IsRequired();
            e.Property(x => x.FileName).HasMaxLength(500).IsRequired();
            e.Property(x => x.StoredFileName).HasMaxLength(500).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
            e.Property(x => x.FileSize).IsRequired();
            e.Property(x => x.IsScreenshot).HasDefaultValue(false).IsRequired();
            e.Property(x => x.FileData).HasColumnType("bytea");

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<JobEntity>()
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .HasConstraintName("FK_TblJobAttachment_TblJob")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.JobId).HasDatabaseName("IX_TblJobAttachment_JobId");
        });

        // ---------------- TblVoyagePortArrival ----------------
        b.Entity<VoyagePortArrivalEntity>(e =>
        {
            e.ToTable("TblVoyagePortArrival");
            e.HasKey(x => x.Id);

            e.Property(x => x.VoyagePortId).IsRequired();
            e.Property(x => x.InboundVoyage).HasMaxLength(50);
            e.Property(x => x.OutboundVoyage).HasMaxLength(50);
            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.LastPortId)
                .HasConstraintName("FK_TblVoyagePortArrival_TblPorts_Last")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.NextPortId)
                .HasConstraintName("FK_TblVoyagePortArrival_TblPorts_Next")
                .OnDelete(DeleteBehavior.NoAction);
            e.Property(x => x.TugsIn).HasMaxLength(50);
            e.Property(x => x.ArrivalDraftFwdMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.ArrivalDraftAftMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.ArrivalDraftMeanMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.FuelOil).HasColumnType("decimal(10,2)");
            e.Property(x => x.DieselOil).HasColumnType("decimal(10,2)");
            e.Property(x => x.FreshWater).HasColumnType("decimal(10,2)");
            e.Property(x => x.BallastWater).HasColumnType("decimal(10,2)");
            e.Property(x => x.Remarks).HasMaxLength(2000);

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<VoyagePortEntity>()
                .WithMany()
                .HasForeignKey(x => x.VoyagePortId)
                .HasConstraintName("FK_TblVoyagePortArrival_TblVoyagePort")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.VoyagePortId)
                .IsUnique()
                .HasDatabaseName("IX_TblVoyagePortArrival_VoyagePortId");
        });

        // ---------------- TblVoyagePortDeparture ----------------
        b.Entity<VoyagePortDepartureEntity>(e =>
        {
            e.ToTable("TblVoyagePortDeparture");
            e.HasKey(x => x.Id);

            e.Property(x => x.VoyagePortId).IsRequired();
            e.Property(x => x.InboundVoyage).HasMaxLength(50);
            e.Property(x => x.OutboundVoyage).HasMaxLength(50);
            e.Property(x => x.TugsOut).HasMaxLength(50);
            e.Property(x => x.DepDraftFwdMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.DepDraftAftMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.DepDraftMeanMtr).HasColumnType("decimal(10,2)");
            e.Property(x => x.FuelOil).HasColumnType("decimal(10,2)");
            e.Property(x => x.DieselOil).HasColumnType("decimal(10,2)");
            e.Property(x => x.FreshWater).HasColumnType("decimal(10,2)");
            e.Property(x => x.BallastWater).HasColumnType("decimal(10,2)");
            e.Property(x => x.Remarks).HasMaxLength(2000);

            e.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
            e.Property(x => x.CreatedOn).IsRequired();
            e.Property(x => x.ModifiedOn).IsRequired();
            e.Property(x => x.CreatedBy).IsRequired();
            e.Property(x => x.ModifiedBy).IsRequired();

            e.HasOne<VoyagePortEntity>()
                .WithMany()
                .HasForeignKey(x => x.VoyagePortId)
                .HasConstraintName("FK_TblVoyagePortDeparture_TblVoyagePort")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<PortsEntity>()
                .WithMany()
                .HasForeignKey(x => x.NextPortId)
                .HasConstraintName("FK_TblVoyagePortDeparture_TblPorts_Next")
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(x => x.VoyagePortId)
                .IsUnique()
                .HasDatabaseName("IX_TblVoyagePortDeparture_VoyagePortId");
        });

    }
}
