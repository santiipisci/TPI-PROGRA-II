using Microsoft.EntityFrameworkCore;
using tpRepository.Common;
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

    public async Task<List<ModeloDto>> GetModelosAsync()
    {
        return await _context.Modelos
                     .OrderBy(m => m.IdMarcaNavigation.Nombre)
                     .Select(m => new ModeloDto(
                         m.IdModelo,
                         m.IdMarca,
                         m.IdMarcaNavigation.Nombre,
                         m.Nombre,
                         m.AnioDesde,
                         m.AnioHasta))
                     .ToListAsync();
    }

    public async Task<bool> ExisteModeloAsync(string marca, string nombre)
    {
        var marcaNombre = marca.Trim();
        var modeloNombre = nombre.Trim();

        return await _context.Modelos
                     .AnyAsync(m => m.Nombre == modeloNombre && m.IdMarcaNavigation.Nombre == marcaNombre);
    }

    public async Task<int> CrearModeloAsync(ModeloCrearDto dto)
    {
        var marcaNombre = dto.Marca.Trim();
        var modeloNombre = dto.Nombre.Trim();

        var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Nombre == marcaNombre);

        if (marca == null)
        {
            marca = new Marca { Nombre = marcaNombre };
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
        }

        if (await _context.Modelos.AnyAsync(m => m.IdMarca == marca.IdMarca && m.Nombre == modeloNombre))
            throw new DuplicateException("Ese modelo ya existe para la marca indicada.");

        var modelo = new Modelo { IdMarca =  marca.IdMarca, Nombre = modeloNombre };
        _context.Modelos.Add(modelo);
        await _context.SaveChangesAsync();

        return modelo.IdModelo;
    }

    public async Task AsociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        var autoparte = await _context.Autopartes
                              .Include(a => a.IdModelos)
                              .FirstOrDefaultAsync(a => a.IdProducto == idProducto);

        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        var modelo = await _context.Modelos.FindAsync(idModelo);

        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        if (!autoparte.IdModelos.Contains(modelo))
        {
            autoparte.IdModelos.Add(modelo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        var autoparte = await _context.Autopartes
                              .Include(a => a.IdModelos)
                              .FirstOrDefaultAsync(a => a.IdProducto == idProducto);

        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        var modelo = await _context.Modelos.FindAsync(idModelo);

        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        if (autoparte.IdModelos.Contains(modelo))
        {
            autoparte.IdModelos.Remove(modelo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto)
    {
        var cambios = 0;

        foreach (var idProducto in dto.IdProductos)
        {
            var autoparte = await _context.Autopartes
                                  .Include(a => a.IdModelos)
                                  .FirstOrDefaultAsync(a => a.IdProducto == idProducto);

            foreach(var idModelo in dto.IdModelos)
            {
                var modelo = await _context.Modelos.FindAsync(idModelo);

                if (autoparte == null || modelo == null) continue;

                var yaAsociado = autoparte.IdModelos.Contains(modelo);

                if (dto.Asociar && !yaAsociado)
                {
                    autoparte.IdModelos.Add(modelo);
                    cambios++;
                }

                if (!dto.Asociar && yaAsociado)
                {
                    autoparte.IdModelos.Remove(modelo);
                    cambios++;
                }
            }
        }

        await _context.SaveChangesAsync();
        return cambios;
    }
}
