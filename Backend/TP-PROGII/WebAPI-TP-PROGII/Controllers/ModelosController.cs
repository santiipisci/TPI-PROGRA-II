using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/modelos")]),
// por eso la clase no lleva [Route].
[ApiController]
public class ModelosController : ControllerBase
{
    private readonly IModelosService _service;

    public ModelosController(IModelosService service)
    {
        _service = service;
    }

    [HttpGet("api/modelos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModelos()
    {
        return Ok(await _service.GetModelosAsync());
    }

    [HttpPost("api/modelos")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> CrearModelo(ModeloCrearDto dto)
    {
        var id = await _service.CrearModeloAsync(dto);
        return StatusCode(201, new { idModelo = id });
    }

    [HttpPut("api/modelos/{idModelo}/autopartes/{idProducto}")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Asociar(int idModelo, int idProducto)
    {
        await _service.AsociarModeloAutoparteAsync(idModelo, idProducto);
        return NoContent();
    }

    [HttpDelete("api/modelos/{idModelo}/autopartes/{idProducto}")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Desasociar(int idModelo, int idProducto)
    {
        await _service.DesasociarModeloAutoparteAsync(idModelo, idProducto);
        return NoContent();
    }

    [HttpPost("api/compatibilidad")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> ActualizarCompatibilidad(CompatibilidadLoteDto dto)
    {
        var cambios = await _service.ActualizarCompatibilidadLoteAsync(dto);
        return Ok(new { cambios });
    }
}
