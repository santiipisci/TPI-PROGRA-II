using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/modelos")]),
// por eso la clase no lleva [Route].
[ApiController]
public class ModelosController : ControllerBase
{
    private readonly IModelosService _service;

    public ModelosController(IModelosService service) => _service = service;

    [HttpGet("api/modelos")]
    public async Task<ActionResult<List<ModeloDto>>> GetModelos() =>
        Ok(await _service.GetModelosAsync());

    [HttpPost("api/modelos")]
    public async Task<IActionResult> Create([FromBody] ModeloCrearDto dto)
    {
        var id = await _service.CrearModeloAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new { idModelo = id });
    }

    [HttpPut("api/modelos/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ModeloEditarDto dto)
    {
        await _service.EditarModeloAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("api/modelos/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarModeloAsync(id);
        return NoContent();
    }
}
