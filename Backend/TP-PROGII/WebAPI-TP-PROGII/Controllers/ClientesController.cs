using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

[ApiController]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpPost("api/clientes/registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registro([FromBody] ClienteRegistroDto dto)
    {
        var id = await _service.RegistrarClienteAsync(dto);
        return StatusCode(201, new { idCliente = id });
    }

    [HttpPost("api/clientes/login")]
    [AllowAnonymous]
    public async Task<ActionResult<ClienteDto>> Login([FromBody] ClienteLoginDto dto)
    {
        var cliente = await _service.LoginClienteAsync(dto);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, cliente.IdCliente.ToString()),
            new(ClaimTypes.Name, cliente.NombreRazonSocial),
        };

        var identity = new ClaimsIdentity(claims, AuthSchemes.Cliente);
        await HttpContext.SignInAsync(
            AuthSchemes.Cliente,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return Ok(cliente);
    }

    [HttpGet("api/clientes/me")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Cliente)]
    public async Task<ActionResult<ClienteDto>> Me()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cliente = await _service.GetClienteAsync(id);

        if (cliente == null)
            return Unauthorized(new { mensaje = "La sesión no es válida." });

        return Ok(cliente);
    }

    [HttpPost("api/clientes/logout")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Cliente)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthSchemes.Cliente);
        return NoContent();
    }

    /// <summary>Listado de clientes para el alta de órdenes (panel de administración).</summary>
    [HttpGet("api/admin/clientes")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<ActionResult<List<ClienteDto>>> GetClientes() =>
        Ok(await _service.GetClientesAsync());
}
