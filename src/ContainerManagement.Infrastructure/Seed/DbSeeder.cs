using ContainerManagement.Application.Security;
using ContainerManagement.Infrastructure.Persistence;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerManagement.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, string fakeProviderBaseUrl)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception)
        {
            // Fallback for environments without migrations aligned to the model
            await db.Database.EnsureCreatedAsync();
        }

        await EnsureAdminAsync(db);
    }

    private static async Task EnsureAdminAsync(AppDbContext db)
    {
        if (await db.Users.AsNoTracking().AnyAsync()) return;

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.Admin);
        if (adminRole == null)
        {
            adminRole = new RoleEntity { Id = Guid.NewGuid(), Name = AppRoles.Admin };
            db.Roles.Add(adminRole);
        }

        var userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.User);
        if (userRole == null)
        {
            userRole = new RoleEntity { Id = Guid.NewGuid(), Name = AppRoles.User };
            db.Roles.Add(userRole);
        }

        var adminId = Guid.NewGuid();
        db.Users.Add(new UserEntity
        {
            Id = adminId,
            UserName = "admin",
            Email = "admin@local.test",
            EmailConfirmed = true,
            PasswordHash = PasswordHasher.Hash("Admin@123"),
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0
        });

        db.UserRoles.Add(new UserRoleEntity { UserId = adminId, RoleId = adminRole.Id });
        db.UserRoles.Add(new UserRoleEntity { UserId = adminId, RoleId = userRole.Id });

        await db.SaveChangesAsync();
    }
}
