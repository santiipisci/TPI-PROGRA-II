using Microsoft.AspNetCore.Mvc;
using tpRepository.Services.Interfaces;

namespace WebAPI_TP_PROGII.Controllers;

// Las rutas se declaran completas en cada acción (ej.: [HttpGet("api/autopartes")]),
// por eso la clase no lleva [Route].
[ApiController]
public class AutopartesController : ControllerBase
{
    private readonly IAutopartesService _service;

    public AutopartesController(IAutopartesService service) => _service = service;
}
