using Microsoft.AspNetCore.Mvc;
using tpRepository.Services.Interfaces;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/categorias")]),
// por eso la clase no lleva [Route].
[ApiController]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogosService _service;

    public CatalogosController(ICatalogosService service) => _service = service;

    [HttpGet("api/tipos-cliente")]
    public async Task<ActionResult<List<TipoClienteDto>>> GetTiposCliente() =>
        Ok(await _service.GetTiposClienteAsync());

    [HttpGet("api/estados-orden")]
    public async Task<ActionResult<List<EstadoOrdenDto>>> GetEstadosOrden() =>
        Ok(await _service.GetEstadosOrdenAsync());

    [HttpGet("api/categorias")]
    public async Task<ActionResult<List<CategoriaDto>>> GetCategorias() =>
        Ok(await _service.GetCategoriasAsync());
}
