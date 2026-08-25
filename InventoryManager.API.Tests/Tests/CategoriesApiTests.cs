using System.Net;
using System.Net.Http.Json;
using InventoryManager.API.DTOs.Categories;
using InventoryManager.API.Tests.Infrastructure;

namespace InventoryManager.API.Tests.Tests;

public sealed class CategoriesApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsCreated()
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new CategoryCreateDto
        {
            Name = "Electronics",
            Description = "Devices and accessories"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CategoryReadDto>();
        Assert.NotNull(created);
        Assert.Equal("Electronics", created!.Name);
    }

    [Fact]
    public async Task GetAllCategories_WithSeededData_ReturnsOk()
    {
        await SeedCategoryAsync("Beverages");
        await SeedCategoryAsync("Electronics");

        var response = await Client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryReadDto>>();
        Assert.NotNull(categories);
        Assert.Equal(2, categories!.Count);
    }

    [Fact]
    public async Task GetCategoryById_WhenExists_ReturnsOk()
    {
        var categoryId = await SeedCategoryAsync("Electronics");

        var response = await Client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category = await response.Content.ReadFromJsonAsync<CategoryReadDto>();
        Assert.NotNull(category);
        Assert.Equal(categoryId, category!.Id);
        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public async Task GetCategoryById_WhenMissing_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/categories/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WhenExists_ReturnsNoContent()
    {
        var categoryId = await SeedCategoryAsync("Electronics");

        var response = await Client.PutAsJsonAsync($"/api/categories/{categoryId}", new CategoryUpdateDto
        {
            Name = "Hardware",
            Description = "Updated"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenExistsAndUnused_ReturnsNoContent()
    {
        var categoryId = await SeedCategoryAsync("Electronics");

        var response = await Client.DeleteAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithInvalidData_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new CategoryCreateDto
        {
            Name = string.Empty,
            Description = new string('a', 501)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsInternalServerError()
    {
        await SeedCategoryAsync("Electronics");

        var response = await Client.PostAsJsonAsync("/api/categories", new CategoryCreateDto
        {
            Name = "Electronics",
            Description = "Duplicate"
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}