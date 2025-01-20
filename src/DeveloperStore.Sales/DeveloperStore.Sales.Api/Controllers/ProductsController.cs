using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RequestProductDto dto)
    {
        var response = await _productService.CreateAsync(dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data); 

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] RequestProductDto dto)
    {
        var response = await _productService.UpdateAsync(id, dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var response = await _productService.DeleteAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response); 

        return StatusCode(response.StatusCode, response); 
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int _page = 1, [FromQuery] int _size = 10, [FromQuery] string? _order = null)
    {
        var response = await _productService.GetAllAsync(_page, _size, _order);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(
        string category,
        [FromQuery] int _page = 1,
        [FromQuery] int _size = 10,
        [FromQuery] string? _order = null)
    {
        var response = await _productService.GetByCategoryAsync(category, _page, _size, _order);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var response = await _productService.GetByIdAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }
}
