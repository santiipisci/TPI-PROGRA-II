using tpRepository.Dtos;
using tpRepository.Models;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Repository.Implementations;

public class ModelosRepository : IModelosRepository
{
    private readonly AutopartesDbContext _context;

    public ModelosRepository(AutopartesDbContext context)
    {
        _context = context;
    }

    public Task<List<ModeloDto>> GetModelosAsync() => throw new NotImplementedException();

    public Task<bool> ExisteModeloAsync(string marca, string nombre) => throw new NotImplementedException();

    public Task<int> CrearModeloAsync(ModeloCrearDto dto) => throw new NotImplementedException();

    public Task AsociarModeloAutoparteAsync(int idModelo, int idProducto) => throw new NotImplementedException();

    public Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto) => throw new NotImplementedException();

    public Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto) => throw new NotImplementedException();
}
