using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Dtos;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
public class AdminController : ControllerBase
{
    private readonly IOrdenesService _service;

    public AdminController(IOrdenesService service) => _service = service;

    [HttpGet("api/admin/resumen")]
    public async Task<ActionResult<ResumenDto>> Resumen() =>
        Ok(await _service.GetResumenAsync());

    [HttpGet("api/admin/ordenes")]
    public async Task<ActionResult<OrdenPaginadaDto>> Ordenes(
        [FromQuery] int? idEstadoOrden,
        [FromQuery] string? q,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 10) =>
        Ok(await _service.GetOrdenesAsync(idEstadoOrden, q, pagina, tamano));
}
