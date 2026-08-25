using InventoryManager.API.DTOs.Products;
using InventoryManager.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // قراءة جميع المنتجات - متاح بدون تسجيل دخول
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId)
    {
        var products = await _productService.GetAllAsync(
            search,
            categoryId);

        return Ok(products);
    }

    // قراءة منتج واحد - متاح بدون تسجيل دخول
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductReadDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // المنتجات منخفضة المخزون - متاح بدون تسجيل دخول
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetLowStock()
    {
        var products = await _productService.GetLowStockAsync();

        return Ok(products);
    }

    // إنشاء منتج - يحتاج تسجيل دخول
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductReadDto>> Create(
        ProductCreateDto dto)
    {
        try
        {
            var product = await _productService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // تعديل منتج - يحتاج تسجيل دخول
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductUpdateDto dto)
    {
        try
        {
            var updated = await _productService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // حذف منتج - يحتاج تسجيل دخول
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);

        if (!deleted)
        {
            return BadRequest(
                "Product does not exist or cannot be deleted.");
        }

        return NoContent();
    }
}