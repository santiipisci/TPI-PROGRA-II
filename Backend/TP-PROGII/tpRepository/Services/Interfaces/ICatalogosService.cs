using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Services.Interfaces;

public interface ICatalogosService
{
    Task<List<CategoriaDto>> GetCategoriasAsync();
    Task<List<TipoClienteDto>> GetTiposClienteAsync();
    Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync();
}
