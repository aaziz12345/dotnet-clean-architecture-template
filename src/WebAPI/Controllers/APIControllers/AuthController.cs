using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Registration failed."));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!, "Registration successful."));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _userService.LoginAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return Unauthorized(ApiResponse.Fail(result.Error ?? "Login failed."));
        }

        return Ok(ApiResponse<AuthResponseDto>.Ok(result.Value!, "Login successful."));
    }
}
