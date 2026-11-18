using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

public class CatalogosService : ICatalogosService
{
    private readonly ICatalogosRepository _repo;

    public CatalogosService(ICatalogosRepository repo) => _repo = repo;
}
