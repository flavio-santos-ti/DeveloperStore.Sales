using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] RequestUserDto dto)
    {
        var response = await _userService.CreateAsync(dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] RequestUserDto dto)
    {
        var response = await _userService.UpdateAsync(id, dto);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var response = await _userService.DeleteAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int _page = 1, [FromQuery] int _size = 10, [FromQuery] string? _order = null)
    {
        var response = await _userService.GetAllAsync(_page, _size, _order);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var response = await _userService.GetByIdAsync(id);

        if (response.IsSuccess)
            return StatusCode(response.StatusCode, response.Data);

        return StatusCode(response.StatusCode, response);
    }
}
