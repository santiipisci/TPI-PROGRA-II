using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

public class CatalogosService : ICatalogosService
{
    private readonly ICatalogosRepository _repository;

    public CatalogosService(ICatalogosRepository repository)
    {
        _repository = repository;
    }

    public Task<List<CategoriaDto>> GetCategoriasAsync()
    {
        return _repository.GetCategoriasAsync();
    }

    public Task<List<TipoClienteDto>> GetTiposClienteAsync()
    {
        return _repository.GetTiposClienteAsync();
    }

    public Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync()
    {
        return _repository.GetEstadosOrdenAsync();
    }
}
