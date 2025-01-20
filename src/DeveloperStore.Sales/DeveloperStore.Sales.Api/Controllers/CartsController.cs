using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RequestCartDto dto)
    {
        var response = await _cartService.CreateAsync(dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] RequestCartDto dto)
    {
        var response = await _cartService.UpdateAsync(id, dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var response = await _cartService.DeleteAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var response = await _cartService.GetByIdAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int _page = 1, [FromQuery] int _size = 10, [FromQuery] string? _order = null)
    {
        var response = await _cartService.GetAllAsync(_page, _size, _order);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }
}
