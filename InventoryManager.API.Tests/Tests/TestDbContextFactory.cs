using InventoryManager.API.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Tests.Tests;

public static class TestDbContextFactory
{
    public static string CreateDatabaseName()
    {
        return $"InventoryManagerTestDb_{Guid.NewGuid():N}";
    }

    public static AppDbContext CreateContext(string databaseName)
    {
        var connectionString =
            $"Server=LAPTOP-KRPQMT1G;" +
            $"Database={databaseName};" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    public static void DeleteDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}