using InventoryManager.API.DTOs.Categories;

namespace InventoryManager.API.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryReadDto>> GetAllAsync();

    Task<CategoryReadDto?> GetByIdAsync(int id);

    Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto);

    Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}