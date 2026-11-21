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

    public async Task<List<ModeloDto>> GetModelosAsync(bool incluirInactivos = false)
    {
        var query = _context.Modelos
            .AsNoTracking()
            .Include(m => m.IdMarcaNavigation)
            .AsQueryable();

        if (!incluirInactivos)
            query = query.Where(m => m.Activo);

        var modelos = await query
            .OrderBy(m => m.IdMarcaNavigation.Nombre)
            .ThenBy(m => m.Nombre)
            .ToListAsync();

        return modelos.Select(m => new ModeloDto(
            m.IdModelo,
            m.IdMarca,
            m.IdMarcaNavigation.Nombre,
            m.Nombre,
            m.AnioDesde,
            m.AnioHasta,
            m.Activo)).ToList();
    }

    public async Task<bool> ExisteModeloAsync(string marca, string nombre)
    {
        var nombreMarca = marca.Trim();
        var nombreModelo = nombre.Trim();

        return await _context.Modelos.AnyAsync(m =>
            m.Activo &&
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

        // Si existe un modelo inactivo con el mismo nombre/marca, se reactiva (baja lógica).
        var inactivo = await _context.Modelos.FirstOrDefaultAsync(m =>
            !m.Activo &&
            m.IdMarca == marca.IdMarca &&
            m.Nombre == nombreModelo);
        if (inactivo != null)
        {
            inactivo.Activo = true;
            inactivo.AnioDesde = dto.AnioDesde;
            inactivo.AnioHasta = dto.AnioHasta;
            await _context.SaveChangesAsync();
            return inactivo.IdModelo;
        }

        var modelo = new Modelo
        {
            IdMarca = marca.IdMarca,
            Nombre = nombreModelo,
            AnioDesde = dto.AnioDesde,
            AnioHasta = dto.AnioHasta,
            Activo = true,
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

        var marca = await _context.Marcas.FirstOrDefaultAsync(m => m.Nombre == nombreMarca);
        if (marca == null)
        {
            marca = new Marca { Nombre = nombreMarca };
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
        }

        if (dto.Activo)
        {
            var duplicado = await _context.Modelos.AnyAsync(m =>
                m.Activo &&
                m.IdModelo != id &&
                m.IdMarca == marca.IdMarca &&
                m.Nombre == nombreModelo);
            if (duplicado)
                throw new BusinessException("El modelo ya existe para esa marca.");
        }

        modelo.IdMarca = marca.IdMarca;
        modelo.Nombre = nombreModelo;
        modelo.AnioDesde = dto.AnioDesde;
        modelo.AnioHasta = dto.AnioHasta;
        modelo.Activo = dto.Activo;

        await _context.SaveChangesAsync();
    }

    public async Task EliminarModeloAsync(int id)
    {
        var modelo = await _context.Modelos
            .FirstOrDefaultAsync(m => m.IdModelo == id);
        if (modelo == null || !modelo.Activo)
            throw new NotFoundException("El modelo no existe.");

        // Baja lógica: se conserva el modelo y sus referencias históricas.
        modelo.Activo = false;
        await _context.SaveChangesAsync();
    }

    public async Task AsociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        var modelo = await _context.Modelos
            .Include(m => m.IdProductos)
            .FirstOrDefaultAsync(m => m.IdModelo == idModelo);
        if (modelo == null || !modelo.Activo)
            throw new NotFoundException("El modelo no existe.");

        var autoparte = await _context.Autopartes
            .Include(a => a.IdProductoNavigation)
            .FirstOrDefaultAsync(a => a.IdProducto == idProducto);
        if (autoparte == null || !autoparte.IdProductoNavigation.Activo)
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

        if (modelo == null || !modelo.Activo)
            throw new NotFoundException("El modelo no existe.");

        var autoparte = modelo.IdProductos.FirstOrDefault(a => a.IdProducto == idProducto);
        if (autoparte == null)
            throw new BusinessException("La autoparte no está asociada a este modelo.");

        modelo.IdProductos.Remove(autoparte);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto)
    {
        var idModelos = (dto.IdModelos ?? new List<int>()).Distinct().ToList();
        var idProductos = (dto.IdProductos ?? new List<int>()).Distinct().ToList();

        if (idModelos.Count == 0 || idProductos.Count == 0)
            return 0;

        var modelos = await _context.Modelos
            .Include(m => m.IdProductos)
            .Where(m => m.Activo && idModelos.Contains(m.IdModelo))
            .ToListAsync();

        if (modelos.Count != idModelos.Count)
            throw new BusinessException("Alguno de los modelos no existe.");

        var autopartes = await _context.Autopartes
            .Include(a => a.IdProductoNavigation)
            .Where(a => a.IdProductoNavigation.Activo && idProductos.Contains(a.IdProducto))
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
