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
}
