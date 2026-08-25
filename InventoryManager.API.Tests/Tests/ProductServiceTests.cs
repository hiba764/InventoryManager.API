using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Products;
using InventoryManager.API.Models;
using InventoryManager.API.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManager.API.Tests.Tests;

public class ProductServiceTests
{
    private static AppDbContext CreateContext()
    {
        var databaseName =
            TestDbContextFactory.CreateDatabaseName();

        return TestDbContextFactory.CreateContext(databaseName);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct()
    {
        await using var context = CreateContext();

        try
        {
            context.Categories.Add(new Category
            {
                Name = "Electronics",
                Description = "Electronic products"
            });

            await context.SaveChangesAsync();

            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var dto = new ProductCreateDto
            {
                Name = "Wireless Mouse",
                Description = "Wireless optical mouse",
                Price = 20.50m,
                Quantity = 100,
                MinimumStock = 10,
                CategoryId = 1
            };

            var result =
                await service.CreateAsync(dto);

            Assert.NotEqual(0, result.Id);
            Assert.Equal("Wireless Mouse", result.Name);
            Assert.Equal(20.50m, result.Price);
            Assert.Equal(100, result.Quantity);
            Assert.Equal(10, result.MinimumStock);
            Assert.Equal(1, result.CategoryId);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct()
    {
        await using var context = CreateContext();

        try
        {
            context.Categories.Add(new Category
            {
                Name = "Electronics"
            });

            context.Products.Add(new Product
            {
                Name = "Keyboard",
                Price = 35m,
                Quantity = 50,
                MinimumStock = 5,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var result =
                await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Keyboard", result.Name);
            Assert.Equal("Electronics", result.CategoryName);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task GetLowStockAsync_ShouldReturnLowStockProducts()
    {
        await using var context = CreateContext();

        try
        {
            context.Categories.Add(new Category
            {
                Name = "Electronics"
            });

            context.Products.AddRange(
                new Product
                {
                    Name = "Mouse",
                    Price = 20m,
                    Quantity = 5,
                    MinimumStock = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Monitor",
                    Price = 200m,
                    Quantity = 50,
                    MinimumStock = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var result =
                await service.GetLowStockAsync();

            var products = result.ToList();

            Assert.Single(products);
            Assert.Equal("Mouse", products[0].Name);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        await using var context = CreateContext();

        try
        {
            context.Categories.Add(new Category
            {
                Name = "Electronics"
            });

            context.Products.Add(new Product
            {
                Name = "Old Mouse",
                Price = 20m,
                Quantity = 30,
                MinimumStock = 5,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var dto = new ProductUpdateDto
            {
                Name = "New Mouse",
                Description = "Updated description",
                Price = 25m,
                MinimumStock = 8,
                CategoryId = 1
            };

            var result =
                await service.UpdateAsync(1, dto);

            Assert.True(result);

            var product =
                await context.Products.FindAsync(1);

            Assert.NotNull(product);
            Assert.Equal("New Mouse", product.Name);
            Assert.Equal(25m, product.Price);
            Assert.Equal(8, product.MinimumStock);
            Assert.Equal(30, product.Quantity);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct_WhenNoStockMovementsExist()
    {
        await using var context = CreateContext();

        try
        {
            context.Categories.Add(new Category
            {
                Name = "Electronics"
            });

            context.Products.Add(new Product
            {
                Name = "Mouse",
                Price = 20m,
                Quantity = 30,
                MinimumStock = 5,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var result =
                await service.DeleteAsync(1);

            Assert.True(result);

            var product =
                await context.Products.FindAsync(1);

            Assert.Null(product);
        }
        finally
        {
            TestDbContextFactory.DeleteDatabase(context);
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        await using var context = CreateContext();

        try
        {
            var service = new ProductService(
                context,
                NullLogger<ProductService>.Instance);

            var dto = new ProductCreateDto
            {
                Name = "Mouse",
                Price = 20m,
                Quantity = 10,
                MinimumStock = 2,
                CategoryId = 999
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