using InventoryManager.API.DTOs.StockMovements;

namespace InventoryManager.API.Interfaces;

public interface IStockMovementService
{
    Task<StockMovementReadDto> CreateAsync(
        StockMovementCreateDto dto);

    Task<IEnumerable<StockMovementReadDto>> GetAllAsync();

    Task<StockMovementReadDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockMovementReadDto>> GetByProductAsync(
        int productId);
}