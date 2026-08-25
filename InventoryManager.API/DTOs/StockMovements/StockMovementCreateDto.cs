using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models;

namespace InventoryManager.API.DTOs.StockMovements;

public class StockMovementCreateDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public StockMovementType MovementType { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}