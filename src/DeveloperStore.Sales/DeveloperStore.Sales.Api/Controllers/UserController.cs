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
}
