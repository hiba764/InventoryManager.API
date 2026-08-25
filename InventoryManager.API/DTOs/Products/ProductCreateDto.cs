using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.DTOs.Products;

public class ProductCreateDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; set; }

    [Required]
    public int CategoryId { get; set; }
}