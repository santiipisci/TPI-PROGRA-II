using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

public class AutopartesService : IAutopartesService
{
    private readonly IAutopartesRepository _repo;

    public AutopartesService(IAutopartesRepository repo) => _repo = repo;
}
