using tpRepository.Dtos;
using tpRepository.Models;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Repository.Implementations;

public class AutopartesRepository : IAutopartesRepository
{
    private readonly AutopartesDbContext _context;

    public AutopartesRepository(AutopartesDbContext context)
    {
        _context = context;
    }

    public Task<List<AutoparteDto>> GetAutopartesAsync() => throw new NotImplementedException();

    public Task<bool> ExisteCodigoAsync(string codigoParte) => throw new NotImplementedException();

    public Task<bool> ExisteAutoparteAsync(int idProducto) => throw new NotImplementedException();

    public Task<int> CrearAutoparteAsync(AutoparteCrearDto dto) => throw new NotImplementedException();

    public Task EditarAutoparteAsync(int idProducto, AutoparteEditarDto dto) => throw new NotImplementedException();

    public Task ActualizarStockAsync(int idProducto, int stockActual) => throw new NotImplementedException();

    public Task EliminarAutoparteAsync(int idProducto) => throw new NotImplementedException();
}
