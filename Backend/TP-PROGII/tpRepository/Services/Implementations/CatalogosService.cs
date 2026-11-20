using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de catálogos (tipos de cliente, estados de orden, categorías).</summary>
public class CatalogosService : ICatalogosService
{
    private readonly ICatalogosRepository _repo;

    public CatalogosService(ICatalogosRepository repo) => _repo = repo;

    public Task<List<TipoClienteDto>> GetTiposClienteAsync() => _repo.GetTiposClienteAsync();

    public Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync() => _repo.GetEstadosOrdenAsync();

    public Task<List<CategoriaDto>> GetCategoriasAsync() => _repo.GetCategoriasAsync();
}
