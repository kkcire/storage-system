using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using StorageSystem.Infrastructure.Data;

namespace StorageSystem.Tests.Infrastructure;

internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<StorageContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new StorageContext(_options);
        context.Database.EnsureCreated();
    }

    public StorageContext CreateContext() => new(_options);

    public void Dispose()
    {
        _connection.Dispose();
    }
}
