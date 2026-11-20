using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IAutopartesService
{
    Task<List<AutoparteDto>> GetAutopartesAsync();
    Task<int> CrearAutoparteAsync(AutoparteCrearDto dto);
    Task EditarAutoparteAsync(int idProducto, AutoparteEditarDto dto);
    Task ActualizarStockAsync(int idProducto, int stockActual);
    Task EliminarAutoparteAsync(int idProducto);
}
