using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(AuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken ct)
        => Ok(await _authService.LoginAsync(request, ct));

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken ct)
        => Ok(await _authService.RefreshAsync(request, ct));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<UserDto> GetCurrentUser()
        => Ok(new UserDto(_currentUser.Id, User.Identity?.Name ?? _currentUser.Email, _currentUser.Email, _currentUser.Role, _currentUser.TenantId, _currentUser.StoreId));
}
