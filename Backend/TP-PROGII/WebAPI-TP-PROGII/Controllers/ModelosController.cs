using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
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
    public async Task<ActionResult<List<ModeloDto>>> GetModelos() =>
        Ok(await _service.GetModelosAsync());

    [HttpPost("api/modelos"), Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Create(ModeloCrearDto dto)
    {
        var id = await _service.CrearModeloAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new { idModelo = id });
    }

    [HttpPut("api/modelos/{id:int}"), Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Update(int id, ModeloEditarDto dto)
    {
        await _service.EditarModeloAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("api/modelos/{id:int}"), Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarModeloAsync(id);
        return NoContent();
    }

    [HttpPut("api/modelos/{idModelo:int}/autopartes/{idProducto:int}"),
     Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Asociar(int idModelo, int idProducto)
    {
        await _service.AsociarModeloAutoparteAsync(idModelo, idProducto);
        return NoContent();
    }

    [HttpDelete("api/modelos/{idModelo:int}/autopartes/{idProducto:int}"),
     Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> Desasociar(int idModelo, int idProducto)
    {
        await _service.DesasociarModeloAutoparteAsync(idModelo, idProducto);
        return NoContent();
    }

    [HttpPost("api/compatibilidad"), Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> ActualizarCompatibilidad(CompatibilidadLoteDto dto)
    {
        var cambios = await _service.ActualizarCompatibilidadLoteAsync(dto);
        return Ok(new { cambios });
    }
}
