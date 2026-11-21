using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IModelosService
{
    Task<List<ModeloDto>> GetModelosAsync(bool incluirInactivos = false);
    Task<int> CrearModeloAsync(ModeloCrearDto dto);
    Task EditarModeloAsync(int id, ModeloEditarDto dto);
    Task EliminarModeloAsync(int id);
    Task AsociarModeloAutoparteAsync(int idModelo, int idProducto);
    Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto);
    Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto);
}
