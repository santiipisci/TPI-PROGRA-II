using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;

namespace tpRepository.Repository.Interfaces
{
    public interface IOrdenesRepository
    {
        Task<ResumenDto> GetResumenAsync();
        Task<OrdenPaginadaDto> GetOrdenesAsync(int? idEstadoOrden, string? q, int pagina, int tamano);
        Task<OrdenDetalleDto?> GetOrdenAsync(int idOrden);
        Task<int> CrearOrdenAsync(OrdenGuardarDto dto);
        Task<bool> EditarOrdenAsync(int idOrden, OrdenGuardarDto dto);
        Task<bool> EliminarOrdenAsync(int idOrden);
        Task<SeguimientoDto> AgregarSeguimientoAsync(int idOrden, SeguimientoCrearDto dto);
        Task<bool> EliminarSeguimientoAsync(int idSeguimiento);
    }
}
