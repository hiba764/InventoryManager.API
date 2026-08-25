using InventoryManager.API.DTOs.Auth;

namespace InventoryManager.API.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);

    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}