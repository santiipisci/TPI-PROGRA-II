using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    [HttpPost("api/auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<AdminDto>> Login([FromBody] AdminLoginDto dto)
    {
        var admin = await _service.LoginAdminAsync(dto);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.IdUsuario.ToString()),
            new(ClaimTypes.Name, admin.Usuario),
            new(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, AuthSchemes.Admin);
        await HttpContext.SignInAsync(
            AuthSchemes.Admin,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
            });

        return Ok(admin);
    }

    [HttpGet("api/auth/me")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<ActionResult<AdminDto>> Me()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, out var id))
            return Unauthorized(new { mensaje = "La sesión no es válida." });

        return Ok(await _service.GetAdminAsync(id));
    }

    [HttpPost("api/auth/logout")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Admin);
        return NoContent();
    }
}
