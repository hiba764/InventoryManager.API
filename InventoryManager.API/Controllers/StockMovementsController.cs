using InventoryManager.API.DTOs.StockMovements;
using InventoryManager.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(
        IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockMovementReadDto>>> GetAll()
    {
        var movements = await _stockMovementService.GetAllAsync();

        return Ok(movements);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StockMovementReadDto>> GetById(int id)
    {
        var movement =
            await _stockMovementService.GetByIdAsync(id);

        if (movement is null)
        {
            return NotFound();
        }

        return Ok(movement);
    }

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<StockMovementReadDto>>> GetByProduct(
        int productId)
    {
        var movements =
            await _stockMovementService.GetByProductAsync(productId);

        return Ok(movements);
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementReadDto>> Create(
        StockMovementCreateDto dto)
    {
        try
        {
            var movement =
                await _stockMovementService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = movement.Id },
                movement);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}