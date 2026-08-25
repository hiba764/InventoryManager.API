using InventoryManager.API.Data;
using InventoryManager.API.DTOs.StockMovements;
using InventoryManager.API.Interfaces;
using InventoryManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services;

public class StockMovementService : IStockMovementService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(
        AppDbContext context,
        ILogger<StockMovementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StockMovementReadDto> CreateAsync(
        StockMovementCreateDto dto)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                p.Id == dto.ProductId);

        if (product is null)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        var userExists = await _context.Users
            .AnyAsync(u =>
                u.Id == dto.UserId);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        if (dto.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (dto.MovementType == StockMovementType.Out)
        {
            if (product.Quantity < dto.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock for product {ProductId}. Requested: {RequestedQuantity}, Available: {AvailableQuantity}",
                    dto.ProductId,
                    dto.Quantity,
                    product.Quantity);

                throw new InvalidOperationException(
                    "Insufficient stock.");
            }

            product.Quantity -= dto.Quantity;
        }
        else
        {
            product.Quantity += dto.Quantity;
        }

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            UserId = dto.UserId,
            Quantity = dto.Quantity,
            MovementType = dto.MovementType,
            CreatedAt = DateTime.UtcNow,
            Note = dto.Note?.Trim()
        };

        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        _logger.LogInformation(
            "Stock movement {MovementId} created. Product: {ProductId}, Type: {MovementType}, Quantity: {Quantity}",
            movement.Id,
            movement.ProductId,
            movement.MovementType,
            movement.Quantity);

        return await GetByIdAsync(movement.Id)
               ?? throw new InvalidOperationException(
                   "Stock movement could not be retrieved.");
    }

    public async Task<IEnumerable<StockMovementReadDto>> GetAllAsync()
    {
        return await _context.StockMovements
            .AsNoTracking()
            .OrderByDescending(sm => sm.CreatedAt)
            .Select(sm => new StockMovementReadDto
            {
                Id = sm.Id,
                ProductId = sm.ProductId,
                ProductName = sm.Product.Name,
                UserId = sm.UserId,
                Username = sm.User.Username,
                Quantity = sm.Quantity,
                MovementType = sm.MovementType,
                CreatedAt = sm.CreatedAt,
                Note = sm.Note
            })
            .ToListAsync();
    }

    public async Task<StockMovementReadDto?> GetByIdAsync(
        int id)
    {
        return await _context.StockMovements
            .AsNoTracking()
            .Where(sm => sm.Id == id)
            .Select(sm => new StockMovementReadDto
            {
                Id = sm.Id,
                ProductId = sm.ProductId,
                ProductName = sm.Product.Name,
                UserId = sm.UserId,
                Username = sm.User.Username,
                Quantity = sm.Quantity,
                MovementType = sm.MovementType,
                CreatedAt = sm.CreatedAt,
                Note = sm.Note
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockMovementReadDto>> GetByProductAsync(
        int productId)
    {
        return await _context.StockMovements
            .AsNoTracking()
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .Select(sm => new StockMovementReadDto
            {
                Id = sm.Id,
                ProductId = sm.ProductId,
                ProductName = sm.Product.Name,
                UserId = sm.UserId,
                Username = sm.User.Username,
                Quantity = sm.Quantity,
                MovementType = sm.MovementType,
                CreatedAt = sm.CreatedAt,
                Note = sm.Note
            })
            .ToListAsync();
    }
}