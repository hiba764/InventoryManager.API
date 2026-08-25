using InventoryManager.API.Data;
using InventoryManager.API.DTOs.StockMovements;
using InventoryManager.API.Models;
using InventoryManager.API.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManager.API.Tests.Tests;

public class StockMovementServiceTests
{
    private static AppDbContext CreateContext()
    {
        var databaseName =
            TestDbContextFactory.CreateDatabaseName();

        return TestDbContextFactory.CreateContext(databaseName);
    }

    private static async Task<(int ProductId, int UserId)>
        SeedBasicDataAsync(
            AppDbContext context,
            int productQuantity = 100)
    {
        var category = new Category
        {
            Name = "Electronics"
        };

        context.Categories.Add(category);

        await context.SaveChangesAsync();

        var user = new User
        {
            Username = "admin",
            PasswordHash = "test-hash",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Wireless Mouse",
            Price = 20.50m,
            Quantity = productQuantity,
            MinimumStock = 10,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);

        await context.SaveChangesAsync();

        return (product.Id, user.Id);
    }

    [Fact]
    public async Task CreateAsync_InMovement_ShouldIncreaseProductQuantity()
    {
        await using var context = CreateContext();

        try
        {
            var (productId, userId) =
                await SeedBasicDataAsync(context, 100);

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = productId,
                UserId = userId,
                Quantity = 50,
                MovementType = StockMovementType.In,
                Note = "New shipment"
            };

            var result = await service.CreateAsync(dto);

            var product =
                await context.Products.FindAsync(productId);

            Assert.NotNull(product);
            Assert.Equal(150, product.Quantity);

            Assert.NotEqual(0, result.Id);
            Assert.Equal(50, result.Quantity);
            Assert.Equal(
                StockMovementType.In,
                result.MovementType);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_OutMovement_ShouldDecreaseProductQuantity()
    {
        await using var context = CreateContext();

        try
        {
            var (productId, userId) =
                await SeedBasicDataAsync(context, 100);

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = productId,
                UserId = userId,
                Quantity = 30,
                MovementType = StockMovementType.Out,
                Note = "Sale"
            };

            var result = await service.CreateAsync(dto);

            var product =
                await context.Products.FindAsync(productId);

            Assert.NotNull(product);
            Assert.Equal(70, product.Quantity);

            Assert.Equal(30, result.Quantity);
            Assert.Equal(
                StockMovementType.Out,
                result.MovementType);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_OutMovementMoreThanStock_ShouldThrow()
    {
        await using var context = CreateContext();

        try
        {
            var (productId, userId) =
                await SeedBasicDataAsync(context, 10);

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = productId,
                UserId = userId,
                Quantity = 20,
                MovementType = StockMovementType.Out,
                Note = "Invalid sale"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));

            var product =
                await context.Products.FindAsync(productId);

            Assert.NotNull(product);
            Assert.Equal(10, product.Quantity);

            var movementCount =
                context.StockMovements.Count();

            Assert.Equal(0, movementCount);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateStockMovementRecord()
    {
        await using var context = CreateContext();

        try
        {
            var (productId, userId) =
                await SeedBasicDataAsync(context, 100);

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = productId,
                UserId = userId,
                Quantity = 25,
                MovementType = StockMovementType.In,
                Note = "Stock received"
            };

            var result = await service.CreateAsync(dto);

            var movement =
                await context.StockMovements.FindAsync(result.Id);

            Assert.NotNull(movement);
            Assert.Equal(productId, movement.ProductId);
            Assert.Equal(userId, movement.UserId);
            Assert.Equal(25, movement.Quantity);
            Assert.Equal(
                StockMovementType.In,
                movement.MovementType);
            Assert.Equal(
                "Stock received",
                movement.Note);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        await using var context = CreateContext();

        try
        {
            var category = new Category
            {
                Name = "Electronics"
            };

            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var user = new User
            {
                Username = "admin",
                PasswordHash = "test-hash",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = 999,
                UserId = user.Id,
                Quantity = 10,
                MovementType = StockMovementType.In
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateAsync(dto));
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await using var context = CreateContext();

        try
        {
            var category = new Category
            {
                Name = "Electronics"
            };

            context.Categories.Add(category);

            await context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Mouse",
                Price = 20m,
                Quantity = 100,
                MinimumStock = 10,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Products.Add(product);

            await context.SaveChangesAsync();

            var service = new StockMovementService(
                context,
                NullLogger<StockMovementService>.Instance);

            var dto = new StockMovementCreateDto
            {
                ProductId = product.Id,
                UserId = 999,
                Quantity = 10,
                MovementType = StockMovementType.In
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateAsync(dto));
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }
}