using Microsoft.EntityFrameworkCore;
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

    public async Task<List<TipoClienteDto>> GetTiposClienteAsync()
    {
        return await _context.TipoClientes
                     .OrderBy(t => t.Descripcion)
                     .Select(t => new TipoClienteDto(t.IdTipoCliente, t.Descripcion))
                     .ToListAsync();
    }

    public async Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync()
    {
        return await _context.EstadoOrdens
                     .Select(e => new EstadoOrdenDto(e.IdEstadoOrden, e.Descripcion))
                     .ToListAsync();
    }

    public async Task<List<CategoriaDto>> GetCategoriasAsync()
    {
        return await _context.CategoriaAutopartes
                     .Select(c => new CategoriaDto(c.IdCategoria, c.Nombre))
                     .ToListAsync();
    }
}
