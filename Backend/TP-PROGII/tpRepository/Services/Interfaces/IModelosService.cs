using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IModelosService
{
    Task<List<ModeloDto>> GetModelosAsync();
    Task<int> CrearModeloAsync(ModeloCrearDto dto);
    Task AsociarModeloAutoparteAsync(int idModelo, int idProducto);
    Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto);
    Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto);
}
