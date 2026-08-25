using InventoryManager.API.DTOs.Products;

namespace InventoryManager.API.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductReadDto>> GetAllAsync(
        string? search = null,
        int? categoryId = null);

    Task<ProductReadDto?> GetByIdAsync(int id);

    Task<ProductReadDto> CreateAsync(ProductCreateDto dto);

    Task<bool> UpdateAsync(int id, ProductUpdateDto dto);

    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<ProductReadDto>> GetLowStockAsync();
}