using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/autopartes")]),
// por eso la clase no lleva [Route].
[ApiController]
public class AutopartesController : ControllerBase
{
    private readonly IAutopartesService _service;

    public AutopartesController(IAutopartesService service) => _service = service;

    [HttpGet("api/autopartes")]
    public async Task<ActionResult<List<AutoparteDto>>> GetAutopartes() =>
        Ok(await _service.GetAutopartesAsync());

    [HttpPost("api/autopartes")]
    public async Task<IActionResult> Create([FromBody] AutoparteCrearDto dto)
    {
        var id = await _service.CrearAutoparteAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new { idProducto = id });
    }

    [HttpPut("api/autopartes/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AutoparteEditarDto dto)
    {
        await _service.EditarAutoparteAsync(id, dto);
        return NoContent();
    }

    [HttpPatch("api/autopartes/{id:int}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int stockActual)
    {
        await _service.ActualizarStockAsync(id, stockActual);
        return NoContent();
    }

    [HttpDelete("api/autopartes/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarAutoparteAsync(id);
        return NoContent();
    }
}
