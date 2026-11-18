using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

public class ModelosService : IModelosService
{
    private readonly IModelosRepository _repo;

    public ModelosService(IModelosRepository repo) => _repo = repo;
}
