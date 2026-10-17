using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StorageSystem.Infrastructure.Data;

public class StorageContextFactory : IDesignTimeDbContextFactory<StorageContext>
{
    public StorageContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StorageContext>()
            .UseSqlite("Data Source=storage.db")
            .Options;

        return new StorageContext(options);
    }
}