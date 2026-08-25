namespace InventoryManager.API.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int MinimumStock { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<StockMovement> StockMovements { get; set; }
        = new List<StockMovement>();
}