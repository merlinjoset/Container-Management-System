using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ContainerManagement.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("..", "ContainerManagement.Web", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("..", "ContainerManagement.Web", "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = cfg.GetConnectionString("Default")
                   ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                   ?? "Server=(localdb)\\MSSQLLocalDB;Database=ContainerManagementDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new AppDbContext(options);
    }
}

