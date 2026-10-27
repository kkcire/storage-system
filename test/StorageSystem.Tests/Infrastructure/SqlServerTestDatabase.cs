using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using StorageSystem.Infrastructure.Data;

namespace StorageSystem.Tests.Infrastructure;

internal sealed class SqlServerTestDatabase : IDisposable
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Encrypt=false";

    private readonly DbContextOptions<StorageContext> _options;

    public SqlServerTestDatabase()
    {
        var connectionBuilder = new SqlConnectionStringBuilder(GetServerConnectionString())
        {
            InitialCatalog = $"StorageSystemTests_{Guid.NewGuid():N}",
            MultipleActiveResultSets = true,
            ConnectTimeout = 5
        };

        _options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        try
        {
            using var context = new StorageContext(_options);
            context.Database.EnsureCreated();
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                "Could not connect to SQL Server for integration tests. " +
                "Set STORAGE_SQLSERVER_TEST_CONNECTION to a reachable SQL Server instance " +
                "or install SQL Server LocalDB.",
                ex);
        }
    }

    public StorageContext CreateContext() => new(_options);

    public void Dispose()
    {
        using var context = new StorageContext(_options);
        context.Database.EnsureDeleted();
    }

    private static string GetServerConnectionString()
        => Environment.GetEnvironmentVariable("STORAGE_SQLSERVER_TEST_CONNECTION")
           ?? DefaultConnectionString;
}
