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

        // Attempt to baseline migrations to avoid duplicate CREATE errors if schema exists
        await DbSeederInternals.TryBaselineInitialMigrationAsync(db);

        await db.Database.MigrateAsync();
     
    }
}

static class DbSeederInternals
{
    public static async Task TryBaselineInitialMigrationAsync(AppDbContext db)
    {
        try
        {
            var applied = await db.Database.GetAppliedMigrationsAsync();
            if (applied.Any()) return;

            // Check for an existing known table indicating schema already exists
            const string checkPaymentProviders = "IF OBJECT_ID(N'[dbo].[PaymentProviders]', N'U') IS NULL SELECT 0 ELSE SELECT 1";
            var exists = 0;

            await using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = checkPaymentProviders;
                if (cmd.Connection!.State != System.Data.ConnectionState.Open)
                    await cmd.Connection.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                exists = Convert.ToInt32(result ?? 0);
            }

            if (exists == 1)
            {
                const string ensureHistory = @"IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [__EFMigrationsHistory](
                          [MigrationId] nvarchar(150) NOT NULL,
                          [ProductVersion] nvarchar(32) NOT NULL,
                          CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        );
                    END;";
                                    await db.Database.ExecuteSqlRawAsync(ensureHistory);

                                    const string baseline = @"IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260202060642_InitialCreate')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20260202060642_InitialCreate', '8.0.8');
                    END;";
                await db.Database.ExecuteSqlRawAsync(baseline);
            }
        }
        catch
        {
           
        }
    }
}
