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
        var tipos = await _context.TipoClientes
            .AsNoTracking()
            .OrderBy(t => t.IdTipoCliente)
            .ToListAsync();

        return tipos.Select(t => new TipoClienteDto(t.IdTipoCliente, t.Descripcion)).ToList();
    }

    public async Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync()
    {
        var estados = await _context.EstadoOrdens
            .AsNoTracking()
            .OrderBy(e => e.IdEstadoOrden)
            .ToListAsync();

        return estados.Select(e => new EstadoOrdenDto(e.IdEstadoOrden, e.Descripcion)).ToList();
    }

    public async Task<List<CategoriaDto>> GetCategoriasAsync()
    {
        var categorias = await _context.CategoriaAutopartes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return categorias.Select(c => new CategoriaDto(c.IdCategoria, c.Nombre)).ToList();
    }
}
