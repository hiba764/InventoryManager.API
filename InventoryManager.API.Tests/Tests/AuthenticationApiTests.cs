using System.Net;
using System.Net.Http.Json;
using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.Tests.Infrastructure;

namespace InventoryManager.API.Tests.Tests;

public sealed class AuthenticationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = "alice",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        await RegisterUserAsync("alice", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = "alice",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await RegisterUserAsync("alice", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = "alice",
            Password = "Password123!"
        });

        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.False(string.IsNullOrWhiteSpace(login?.Token));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        await RegisterUserAsync("alice", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Username = "alice",
            Password = "WrongPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ReturnsBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = "",
            Password = "123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}