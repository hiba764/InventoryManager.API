using System.Net;
using System.Net.Http.Json;
using InventoryManager.API.DTOs.StockMovements;
using InventoryManager.API.Models;
using InventoryManager.API.Tests.Infrastructure;

namespace InventoryManager.API.Tests.Tests;

public sealed class StockMovementsApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateStockMovement_WithoutAuth_ReturnsUnauthorized()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var userId = await SeedUserAsync("alice", "test-hash");
        var productId = await SeedProductAsync(categoryId, "Mouse");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 10,
            MovementType = StockMovementType.In,
            Note = "Received"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStockMovement_InMovement_IncreasesProductQuantity()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var userId = await SeedUserAsync("alice", "test-hash");
        var productId = await SeedProductAsync(categoryId, "Mouse", quantity: 100);
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 25,
            MovementType = StockMovementType.In,
            Note = "Incoming stock"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var productResponse = await Client.GetAsync($"/api/products/{productId}");
        var product = await productResponse.Content.ReadFromJsonAsync<InventoryManager.API.DTOs.Products.ProductReadDto>();

        Assert.NotNull(product);
        Assert.Equal(125, product!.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_OutMovement_DecreasesProductQuantity()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var userId = await SeedUserAsync("alice", "test-hash");
        var productId = await SeedProductAsync(categoryId, "Mouse", quantity: 100);
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 30,
            MovementType = StockMovementType.Out,
            Note = "Sale"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var productResponse = await Client.GetAsync($"/api/products/{productId}");
        var product = await productResponse.Content.ReadFromJsonAsync<InventoryManager.API.DTOs.Products.ProductReadDto>();

        Assert.NotNull(product);
        Assert.Equal(70, product!.Quantity);
    }

    [Fact]
    public async Task CreateStockMovement_WhenQuantityExceedsStock_ReturnsBadRequest()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var userId = await SeedUserAsync("alice", "test-hash");
        var productId = await SeedProductAsync(categoryId, "Mouse", quantity: 10);
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 20,
            MovementType = StockMovementType.Out,
            Note = "Too many"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStockMovement_WhenProductDoesNotExist_ReturnsBadRequest()
    {
        var userId = await SeedUserAsync("alice", "test-hash");
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = 999,
            UserId = userId,
            Quantity = 10,
            MovementType = StockMovementType.In,
            Note = "Missing product"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStockMovement_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var productId = await SeedProductAsync(categoryId, "Mouse");
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = 999,
            Quantity = 10,
            MovementType = StockMovementType.In,
            Note = "Missing user"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateStockMovement_WithInvalidPayload_ReturnsBadRequest()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var userId = await SeedUserAsync("alice", "test-hash");
        var productId = await SeedProductAsync(categoryId, "Mouse");
        await RegisterUserAsync("alice-login", "Password123!");
        await LoginAndSetBearerTokenAsync("alice-login", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/stockmovements", new StockMovementCreateDto
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 0,
            MovementType = StockMovementType.In,
            Note = new string('a', 501)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}