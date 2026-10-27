using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StorageSystem.Infrastructure.Data;

public class StorageContextFactory : IDesignTimeDbContextFactory<StorageContext>
{
    public StorageContext CreateDbContext(string[] args)
    {
        var startupProjectPath = ResolveStartupProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(startupProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("STORAGE_SQLSERVER_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=StorageSystemDesignTime;Integrated Security=true;TrustServerCertificate=true;Encrypt=false";

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StorageContext(options);
    }

    private static string ResolveStartupProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidatePaths = new[]
        {
            Path.Combine(currentDirectory, "src", "StorageSystem.Console"),
            Path.Combine(currentDirectory, "..", "StorageSystem.Console"),
            Path.Combine(currentDirectory, "..", "..", "src", "StorageSystem.Console")
        };

        foreach (var candidatePath in candidatePaths.Select(Path.GetFullPath))
        {
            if (File.Exists(Path.Combine(candidatePath, "StorageSystem.Console.csproj")))
                return candidatePath;
        }

        return currentDirectory;
    }
}