using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Products;
using InventoryManager.API.Interfaces;
using InventoryManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        AppDbContext context,
        ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductReadDto>> GetAllAsync(
        string? search = null,
        int? categoryId = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductReadDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductReadDto> CreateAsync(
        ProductCreateDto dto)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "Category not found.");
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            Quantity = dto.Quantity,
            MinimumStock = dto.MinimumStock,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} created successfully.",
            product.Id);

        return await GetByIdAsync(product.Id)
               ?? throw new InvalidOperationException(
                   "Product could not be retrieved after creation.");
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductUpdateDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return false;
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "Category not found.");
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.MinimumStock = dto.MinimumStock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} updated successfully.",
            id);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.StockMovements)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return false;
        }

        if (product.StockMovements.Any())
        {
            _logger.LogWarning(
                "Product {ProductId} cannot be deleted because it has stock movements.",
                id);

            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} deleted successfully.",
            id);

        return true;
    }

    public async Task<IEnumerable<ProductReadDto>> GetLowStockAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Quantity <= p.MinimumStock)
            .OrderBy(p => p.Quantity)
            .Select(p => new ProductReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved {Count} low-stock products.",
            products.Count);

        return products;
    }
}