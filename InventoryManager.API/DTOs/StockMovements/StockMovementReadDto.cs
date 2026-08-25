using InventoryManager.API.Models;

namespace InventoryManager.API.DTOs.StockMovements;

public class StockMovementReadDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public StockMovementType MovementType { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }
}