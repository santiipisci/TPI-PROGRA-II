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
        var modelos = await _context.Modelos
            .AsNoTracking()
            .Include(m => m.IdMarcaNavigation)
            .OrderBy(m => m.IdMarcaNavigation.Nombre)
            .ThenBy(m => m.Nombre)
            .ToListAsync();

        return modelos.Select(m => new ModeloDto(
            m.IdModelo,
            m.IdMarca,
            m.IdMarcaNavigation.Nombre,
            m.Nombre,
            m.AnioDesde,
            m.AnioHasta)).ToList();
    }

    public async Task<bool> ExisteModeloAsync(string marca, string nombre)
    {
        var nombreMarca = marca.Trim();
        var nombreModelo = nombre.Trim();

        return await _context.Modelos.AnyAsync(m =>
            m.IdMarcaNavigation.Nombre == nombreMarca &&
            m.Nombre == nombreModelo);
    }

    public async Task<int> CrearModeloAsync(ModeloCrearDto dto)
    {
        var nombreMarca = dto.Marca.Trim();
        var nombreModelo = dto.Nombre.Trim();

        if (await ExisteModeloAsync(nombreMarca, nombreModelo))
            throw new BusinessException("El modelo ya existe para esa marca.");

        var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Nombre == nombreMarca);

        if (marca == null)
        {
            marca = new Marca { Nombre = nombreMarca };
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
        }

        var modelo = new Modelo
        {
            IdMarca = marca.IdMarca,
            Nombre = nombreModelo,
            AnioDesde = dto.AnioDesde,
            AnioHasta = dto.AnioHasta,
        };

        _context.Modelos.Add(modelo);
        await _context.SaveChangesAsync();

        return modelo.IdModelo;
    }

    public async Task EditarModeloAsync(int id, ModeloEditarDto dto)
    {
        var modelo = await _context.Modelos.FirstOrDefaultAsync(m => m.IdModelo == id);
        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        var nombreMarca = dto.Marca.Trim();
        var nombreModelo = dto.Nombre.Trim();

        var duplicado = await _context.Modelos.AnyAsync(m =>
            m.IdModelo != id &&
            m.IdMarcaNavigation.Nombre == nombreMarca &&
            m.Nombre == nombreModelo);
        if (duplicado)
            throw new BusinessException("El modelo ya existe para esa marca.");

        var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Nombre == nombreMarca);
        if (marca == null)
        {
            marca = new Marca { Nombre = nombreMarca };
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
        }

        modelo.IdMarca = marca.IdMarca;
        modelo.Nombre = nombreModelo;
        modelo.AnioDesde = dto.AnioDesde;
        modelo.AnioHasta = dto.AnioHasta;

        await _context.SaveChangesAsync();
    }

    public async Task EliminarModeloAsync(int id)
    {
        var modelo = await _context.Modelos
            .Include(m => m.IdProductos)
            .Include(m => m.Vehiculos)
            .FirstOrDefaultAsync(m => m.IdModelo == id);
        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        if (modelo.Vehiculos.Count > 0)
            throw new BusinessException("No se puede eliminar el modelo porque tiene vehículos registrados.");

        // Quita primero las asociaciones con autopartes (tabla intermedia).
        modelo.IdProductos.Clear();
        _context.Modelos.Remove(modelo);
        await _context.SaveChangesAsync();
    }

    public async Task AsociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        var modelo = await _context.Modelos.FirstOrDefaultAsync(m => m.IdModelo == idModelo);
        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        var autoparte = await _context.Autopartes.FirstOrDefaultAsync(a => a.IdProducto == idProducto);
        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        if (modelo.IdProductos.Any(a => a.IdProducto == idProducto))
            throw new BusinessException("La autoparte ya está asociada a este modelo.");

        modelo.IdProductos.Add(autoparte);
        await _context.SaveChangesAsync();
    }

    public async Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        var modelo = await _context.Modelos
            .Include(m => m.IdProductos)
            .FirstOrDefaultAsync(m => m.IdModelo == idModelo);

        if (modelo == null)
            throw new NotFoundException("El modelo no existe.");

        var autoparte = modelo.IdProductos.FirstOrDefault(a => a.IdProducto == idProducto);
        if (autoparte == null)
            throw new BusinessException("La autoparte no está asociada a este modelo.");

        modelo.IdProductos.Remove(autoparte);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto)
    {
        var idModelos = dto.IdModelos ?? new List<int>();
        var idProductos = dto.IdProductos ?? new List<int>();

        if (idModelos.Count == 0 || idProductos.Count == 0)
            return 0;

        var modelos = await _context.Modelos
            .Include(m => m.IdProductos)
            .Where(m => idModelos.Contains(m.IdModelo))
            .ToListAsync();

        if (modelos.Count != idModelos.Count)
            throw new BusinessException("Alguno de los modelos no existe.");

        var autopartes = await _context.Autopartes
            .Where(a => idProductos.Contains(a.IdProducto))
            .ToListAsync();

        if (autopartes.Count != idProductos.Count)
            throw new BusinessException("Alguna de las autopartes no existe.");

        var cambios = 0;

        if (dto.Asociar)
        {
            foreach (var modelo in modelos)
            {
                foreach (var autoparte in autopartes)
                {
                    if (!modelo.IdProductos.Any(a => a.IdProducto == autoparte.IdProducto))
                    {
                        modelo.IdProductos.Add(autoparte);
                        cambios++;
                    }
                }
            }
        }
        else
        {
            foreach (var modelo in modelos)
            {
                var autopartesARemover = modelo.IdProductos
                    .Where(a => idProductos.Contains(a.IdProducto))
                    .ToList();

                foreach (var autoparte in autopartesARemover)
                {
                    modelo.IdProductos.Remove(autoparte);
                    cambios++;
                }
            }
        }

        if (cambios > 0)
            await _context.SaveChangesAsync();

        return cambios;
    }
}
