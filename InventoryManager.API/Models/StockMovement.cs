namespace InventoryManager.API.Models;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int Quantity { get; set; }

    public StockMovementType MovementType { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }

    public Product Product { get; set; } = null!;

    public User User { get; set; } = null!;
}