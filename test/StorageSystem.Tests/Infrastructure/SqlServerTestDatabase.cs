using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StorageSystem.Infrastructure.Data;

namespace StorageSystem.Tests.Infrastructure;

/// <summary>
/// Cria um banco SQL Server isolado (nome único) por instância, aplica as
/// migrations reais e o remove no Dispose. Pensada para ser usada como
/// IClassFixture: uma instância por classe de teste, compartilhada entre
/// os [Fact] dela.
/// </summary>
internal sealed class SqlServerTestDatabase : IDisposable
{
    private readonly DbContextOptions<StorageContext> _options;
    private bool _disposed;

    public SqlServerTestDatabase()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SqlServerTestDatabase>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration["STORAGE_SQLSERVER_TEST_CONNECTION"]
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "SQL Server test connection string was not found. " +
                "Configure ConnectionStrings:DefaultConnection in User Secrets " +
                "or set STORAGE_SQLSERVER_TEST_CONNECTION.");

        var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"StorageSystemTests_{Guid.NewGuid():N}",
            MultipleActiveResultSets = true,
            ConnectTimeout = 30,
            // Evita que o connection pool mantenha conexões abertas com o
            // banco de teste depois que o Dispose tenta removê-lo.
            Pooling = false
        };

        _options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        using var context = new StorageContext(_options);
        context.Database.Migrate();
    }

    public StorageContext CreateContext() => new(_options);

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            using var context = new StorageContext(_options);
            context.Database.EnsureDeleted();
        }
        catch
        {
            // Falha ao limpar não deve derrubar testes que já passaram.
            // Pode deixar um banco órfão (StorageSystemTests_*) no servidor;
            // limpeza manual é aceitável em ambiente de desenvolvimento.
        }
        finally
        {
            _disposed = true;
        }
    }
}