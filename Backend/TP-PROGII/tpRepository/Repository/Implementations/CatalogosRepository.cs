using tpRepository.Dtos;
using tpRepository.Models;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Repository.Implementations;

// Era "internal": tiene que ser public para poder registrarla en Program.cs (otro proyecto).
public class CatalogosRepository : ICatalogosRepository
{
    private readonly AutopartesDbContext _context;

    public CatalogosRepository(AutopartesDbContext context)
    {
        _context = context;
    }

    public Task<List<TipoClienteDto>> GetTiposClienteAsync() => throw new NotImplementedException();

    public Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync() => throw new NotImplementedException();

    public Task<List<CategoriaDto>> GetCategoriasAsync() => throw new NotImplementedException();
}
