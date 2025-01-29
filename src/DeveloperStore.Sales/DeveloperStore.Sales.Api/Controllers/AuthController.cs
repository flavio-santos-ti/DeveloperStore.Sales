using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Domain.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DeveloperStore.Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpOptions]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] AuthRequestDto dto)
    {
        var response = await _authService.AuthenticateAsync(dto);

        if (response.IsSuccess)
            return Ok(response.Data);

        return Unauthorized(response);
    }
}
