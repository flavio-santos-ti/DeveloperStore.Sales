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

}
