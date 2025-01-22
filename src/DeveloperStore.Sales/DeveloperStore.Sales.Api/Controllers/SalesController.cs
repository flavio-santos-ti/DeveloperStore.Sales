using DeveloperStore.Sales.Domain.Dtos.Sale;
using DeveloperStore.Sales.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RequestSaleDto dto)
    {
        var response = await _saleService.CreateAsync(dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{saleId}")]
    public async Task<IActionResult> UpdateAsync(int saleId, [FromBody] RequestSaleDto dto)
    {
        var response = await _saleService.UpdateSaleAsync(saleId, dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Message);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{saleId}")]
    public async Task<IActionResult> CancelAsync(int saleId)
    {
        var response = await _saleService.CancelSaleAsync(saleId);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Message);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{saleId}/items/{itemId}")]
    public async Task<IActionResult> CancelItemAsync(int saleId, int itemId)
    {
        var response = await _saleService.CancelSaleItemAsync(saleId, itemId);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Message);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("checkout/{cartId}")]
    public async Task<IActionResult> CheckoutCartAsync(int cartId)
    {
        var response = await _saleService.CheckoutCartAsync(cartId);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }
}
