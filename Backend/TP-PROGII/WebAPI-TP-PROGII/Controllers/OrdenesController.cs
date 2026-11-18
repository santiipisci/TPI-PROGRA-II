using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

/// <summary>ABMC de la tabla maestra de la transacción (ORDEN_PEDIDO) y su seguimiento.</summary>
[ApiController]
[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenesService _service;

    public OrdenesController(IOrdenesService service) => _service = service;

    [HttpGet("api/ordenes/{id:int}")]
    public async Task<ActionResult<OrdenDetalleDto>> Get(int id) =>
        Ok(await _service.GetOrdenAsync(id));

    [HttpPost("api/ordenes")]
    public async Task<IActionResult> Create([FromBody] OrdenGuardarDto dto)
    {
        var id = await _service.CrearOrdenAsync(dto);
        return Created($"/api/ordenes/{id}", new { idOrdenPedido = id });
    }

    [HttpPut("api/ordenes/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrdenGuardarDto dto)
    {
        await _service.EditarOrdenAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("api/ordenes/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarOrdenAsync(id);
        return NoContent();
    }

    [HttpPost("api/ordenes/{id:int}/seguimiento")]
    public async Task<ActionResult<SeguimientoDto>> AgregarSeguimiento(int id, [FromBody] SeguimientoCrearDto dto)
    {
        var seguimiento = await _service.AgregarSeguimientoAsync(id, dto);
        return StatusCode(StatusCodes.Status201Created, seguimiento);
    }

    [HttpDelete("api/seguimiento/{id:int}")]
    public async Task<IActionResult> EliminarSeguimiento(int id)
    {
        await _service.EliminarSeguimientoAsync(id);
        return NoContent();
    }
}
