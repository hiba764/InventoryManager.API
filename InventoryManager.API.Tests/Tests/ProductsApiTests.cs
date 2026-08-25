using System.Net;
using System.Net.Http.Json;
using InventoryManager.API.DTOs.Products;
using InventoryManager.API.Tests.Infrastructure;

namespace InventoryManager.API.Tests.Tests;

public sealed class ProductsApiTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllProducts_WithSeededData_ReturnsOk()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        await SeedProductAsync(categoryId, "Mouse", quantity: 5, minimumStock: 10);
        await SeedProductAsync(categoryId, "Keyboard", quantity: 20, minimumStock: 5);

        var response = await Client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductReadDto>>();
        Assert.NotNull(products);
        Assert.Equal(2, products!.Count);
    }

    [Fact]
    public async Task GetProductById_WhenExists_ReturnsOk()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var productId = await SeedProductAsync(categoryId, "Mouse");

        var response = await Client.GetAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductReadDto>();
        Assert.NotNull(product);
        Assert.Equal(productId, product!.Id);
        Assert.Equal("Mouse", product.Name);
    }

    [Fact]
    public async Task GetProductById_WhenMissing_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLowStock_ReturnsLowStockProducts()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        await SeedProductAsync(categoryId, "Mouse", quantity: 5, minimumStock: 10);
        await SeedProductAsync(categoryId, "Keyboard", quantity: 20, minimumStock: 5);

        var response = await Client.GetAsync("/api/products/low-stock");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductReadDto>>();
        Assert.NotNull(products);
        Assert.Single(products!);
        Assert.Equal("Mouse", products[0].Name);
    }

    [Fact]
    public async Task CreateProduct_WithValidDataAndAuth_ReturnsCreated()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        await RegisterUserAsync("alice", "Password123!");
        await LoginAndSetBearerTokenAsync("alice", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/products", new ProductCreateDto
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 20.5m,
            Quantity = 100,
            MinimumStock = 10,
            CategoryId = categoryId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        var categoryId = await SeedCategoryAsync("Electronics");

        var response = await Client.PostAsJsonAsync("/api/products", new ProductCreateDto
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 20.5m,
            Quantity = 100,
            MinimumStock = 10,
            CategoryId = categoryId
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithMissingCategory_ReturnsBadRequest()
    {
        await RegisterUserAsync("alice", "Password123!");
        await LoginAndSetBearerTokenAsync("alice", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/products", new ProductCreateDto
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 20.5m,
            Quantity = 100,
            MinimumStock = 10,
            CategoryId = 999
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithValidDataAndAuth_ReturnsNoContent()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var productId = await SeedProductAsync(categoryId, "Mouse", quantity: 100, minimumStock: 10);
        await RegisterUserAsync("alice", "Password123!");
        await LoginAndSetBearerTokenAsync("alice", "Password123!");

        var response = await Client.PutAsJsonAsync($"/api/products/{productId}", new ProductUpdateDto
        {
            Name = "Mouse Pro",
            Description = "Updated",
            Price = 30m,
            MinimumStock = 5,
            CategoryId = categoryId
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithAuth_ReturnsNoContent()
    {
        var categoryId = await SeedCategoryAsync("Electronics");
        var productId = await SeedProductAsync(categoryId, "Mouse");
        await RegisterUserAsync("alice", "Password123!");
        await LoginAndSetBearerTokenAsync("alice", "Password123!");

        var response = await Client.DeleteAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}