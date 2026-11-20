using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface ICatalogosService
{
    Task<List<TipoClienteDto>> GetTiposClienteAsync();
    Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync();
    Task<List<CategoriaDto>> GetCategoriasAsync();
}
