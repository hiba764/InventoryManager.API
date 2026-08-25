using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.Interfaces;
using InventoryManager.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManager.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        var username = dto.Username.Trim();

        var exists = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (exists)
        {
            throw new InvalidOperationException(
                "Username already exists.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var username = dto.Username.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return null;
        }

        var validPassword =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

        if (!validPassword)
        {
            return null;
        }

        var token = GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token
        };
    }

    private string GenerateToken(User user)
    {
        var jwt = _configuration.GetSection("Jwt");

        var key = jwt["Key"]
                  ?? throw new InvalidOperationException(
                      "JWT key is missing.");

        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];

        var duration = int.Parse(
            jwt["DurationInMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username)
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(duration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}