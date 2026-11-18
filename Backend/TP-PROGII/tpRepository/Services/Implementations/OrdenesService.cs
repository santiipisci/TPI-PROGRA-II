using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de órdenes, seguimiento y dashboard.</summary>
public class OrdenesService : IOrdenesService
{
    private readonly IOrdenesRepository _repo;

    public OrdenesService(IOrdenesRepository repo) => _repo = repo;

    /* ================================================================
       DASHBOARD
       ================================================================ */

    public Task<ResumenDto> GetResumenAsync() => _repo.GetResumenAsync();

    /* ================================================================
       ÓRDENES Y SEGUIMIENTO
       ================================================================ */

    public Task<OrdenPaginadaDto> GetOrdenesAsync(int? idEstadoOrden, string? q, int pagina, int tamano) =>
        _repo.GetOrdenesAsync(idEstadoOrden, q, pagina, tamano);

    public async Task<OrdenDetalleDto> GetOrdenAsync(int idOrden) =>
        await _repo.GetOrdenAsync(idOrden)
        ?? throw new NotFoundException("La orden no existe.");

    public async Task<int> CrearOrdenAsync(OrdenGuardarDto dto)
    {
        if (dto.FechaPedido == default)
            throw new BusinessException("La fecha del pedido es obligatoria.");

        return await _repo.CrearOrdenAsync(dto);
    }

    public async Task EditarOrdenAsync(int idOrden, OrdenGuardarDto dto)
    {
        if (dto.FechaPedido == default)
            throw new BusinessException("La fecha del pedido es obligatoria.");

        if (!await _repo.EditarOrdenAsync(idOrden, dto))
            throw new NotFoundException("La orden no existe.");
    }

    public async Task EliminarOrdenAsync(int idOrden)
    {
        if (!await _repo.EliminarOrdenAsync(idOrden))
            throw new NotFoundException("La orden no existe.");
    }

    public async Task<SeguimientoDto> AgregarSeguimientoAsync(int idOrden, SeguimientoCrearDto dto)
    {
        if (dto.Fecha == default)
            throw new BusinessException("La fecha del seguimiento es obligatoria.");

        return await _repo.AgregarSeguimientoAsync(idOrden, dto);
    }

    public async Task EliminarSeguimientoAsync(int idSeguimiento)
    {
        if (!await _repo.EliminarSeguimientoAsync(idSeguimiento))
            throw new NotFoundException("El seguimiento no existe.");
    }
}
