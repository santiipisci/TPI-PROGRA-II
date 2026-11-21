using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IOrdenesService
{
    /* ---------- Dashboard ---------- */
    Task<ResumenDto> GetResumenAsync();

    /* ---------- Órdenes y seguimiento ---------- */
    Task<OrdenPaginadaDto> GetOrdenesAsync(int? idEstadoOrden, string? q, int pagina, int tamano, bool incluirInactivos = false);
    Task<OrdenDetalleDto> GetOrdenAsync(int idOrden, bool incluirInactivos = false);
    Task<int> CrearOrdenAsync(OrdenGuardarDto dto);
    Task EditarOrdenAsync(int idOrden, OrdenGuardarDto dto);
    Task EliminarOrdenAsync(int idOrden);
    Task<SeguimientoDto> AgregarSeguimientoAsync(int idOrden, SeguimientoCrearDto dto);
    Task EliminarSeguimientoAsync(int idSeguimiento);
}
