using System.Net.Http.Headers;
using System.Net.Http.Json;
using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.DTOs.Categories;
using InventoryManager.API.DTOs.Products;
using InventoryManager.API.DTOs.StockMovements;
using InventoryManager.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManager.API.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected InventoryApiFactory Factory { get; } = new();

    protected HttpClient Client { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        Client = Factory.CreateClient();
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    protected static async Task<HttpResponseMessage> PostJsonAsync<T>(
        HttpClient client,
        string url,
        T payload)
    {
        return await client.PostAsJsonAsync(url, payload);
    }

    protected static async Task<HttpResponseMessage> PutJsonAsync<T>(
        HttpClient client,
        string url,
        T payload)
    {
        return await client.PutAsJsonAsync(url, payload);
    }

    protected static async Task<LoginResponseDto?> ReadLoginResponseAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }

    private IServiceScope CreateScope()
    {
        var scopeFactory = (IServiceScopeFactory?)Factory.Services.GetService(typeof(IServiceScopeFactory));

        return scopeFactory?.CreateScope()
               ?? throw new InvalidOperationException("Test service scope factory was not available.");
    }

    protected async Task RegisterUserAsync(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode();
    }

    protected async Task<string> LoginAndSetBearerTokenAsync(
        string username,
        string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = username,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = login?.Token ?? throw new InvalidOperationException("Login did not return a token.");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    protected async Task<int> SeedCategoryAsync(string name = "Electronics", string? description = null)
    {
        var category = new Category
        {
            Name = name,
            Description = description
        };

        using var scope = CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetService(typeof(AppDbContext))!;

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category.Id;
    }

    protected async Task<int> SeedUserAsync(string username = "admin", string passwordHash = "test-hash")
    {
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        using var scope = CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetService(typeof(AppDbContext))!;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    protected async Task<int> SeedProductAsync(
        int categoryId,
        string name = "Mouse",
        decimal price = 20m,
        int quantity = 100,
        int minimumStock = 10,
        string? description = null)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Quantity = quantity,
            MinimumStock = minimumStock,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow
        };

        using var scope = CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetService(typeof(AppDbContext))!;

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    protected async Task<int> SeedStockMovementAsync(
        int productId,
        int userId,
        int quantity,
        StockMovementType type,
        string? note = null)
    {
        var movement = new StockMovement
        {
            ProductId = productId,
            UserId = userId,
            Quantity = quantity,
            MovementType = type,
            CreatedAt = DateTime.UtcNow,
            Note = note
        };

        using var scope = CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetService(typeof(AppDbContext))!;

        context.StockMovements.Add(movement);
        await context.SaveChangesAsync();

        return movement.Id;
    }

}