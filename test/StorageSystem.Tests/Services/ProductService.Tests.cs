using Microsoft.Data.Sqlite;
using StorageSystem.Data;
using StorageSystem.Entities;
using StorageSystem.Services;

namespace StorageSystem.Services;

public class ProductServiceTests
{
    private readonly StorageContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var connection = new SqliteConnection("Data Souce = :memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite(connection)
            .Options;

        _context = new(options);
        _context.Database.EnsureCreated();

        _service = new(_context);

    }
    
}