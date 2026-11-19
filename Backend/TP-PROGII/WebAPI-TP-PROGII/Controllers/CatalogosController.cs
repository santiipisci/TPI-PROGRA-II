using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpRepository.Services.Interfaces;
using WebAPI_TP_PROGII.Security;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/categorias")]),
// por eso la clase no lleva [Route].
[ApiController]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogosService _service;

    public CatalogosController(ICatalogosService service)
    {
        _service = service;
    }

    [HttpGet("api/categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        return Ok(await _service.GetCategoriasAsync());
    }

    [HttpGet("api/tipos-cliente")]
    public async Task<IActionResult> GetTiposCliente()
    {
        return Ok(await _service.GetTiposClienteAsync());
    }

    [HttpGet("api/estados-orden")]
    [Authorize(AuthenticationSchemes = AuthSchemes.Admin)]
    public async Task<IActionResult> GetEstadosOrden()
    {
        return Ok(await _service.GetEstadosOrdenAsync());
    }
}
