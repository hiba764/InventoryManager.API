using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}