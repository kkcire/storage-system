using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StorageSystem.Infrastructure.Data;

public class StorageContextFactory : IDesignTimeDbContextFactory<StorageContext>
{
    public StorageContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<StorageContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. " +
                "Set it in the User Secrets (same UserSecretsId as StorageSystem.Console) " +
                "or in the ConnectionStrings__DefaultConnection environment variable.");
        }

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StorageContext(options);
    }
}