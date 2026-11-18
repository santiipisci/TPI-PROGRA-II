using Microsoft.AspNetCore.Mvc;
using tpRepository.Services.Interfaces;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/modelos")]),
// por eso la clase no lleva [Route].
[ApiController]
public class ModelosController : ControllerBase
{
    private readonly IModelosService _service;

    public ModelosController(IModelosService service) => _service = service;
}
