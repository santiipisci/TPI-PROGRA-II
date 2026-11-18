using Microsoft.EntityFrameworkCore;
using tpRepository.Common;

namespace WebAPI_TP_PROGII.Middleware;

/// <summary>
/// Traduce las excepciones de negocio a respuestas HTTP con el formato
/// <c>{ "mensaje": "..." }</c> que espera el front.
/// </summary>
public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, mensaje) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                DuplicateException => (StatusCodes.Status409Conflict, ex.Message),
                BusinessException => (StatusCodes.Status400BadRequest, ex.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message),
                DbUpdateException => (StatusCodes.Status409Conflict,
                    "No se pudo guardar el cambio: el registro entra en conflicto con uno existente."),
                _ => (StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado en el servidor."),
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("La respuesta ya había comenzado; no se puede escribir el error.");
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new { mensaje });
        }
    }
}
