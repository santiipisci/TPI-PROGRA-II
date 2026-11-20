using Microsoft.EntityFrameworkCore;
using tpRepository.Common;
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

    public async Task<List<AutoparteDto>> GetAutopartesAsync()
    {
        var autopartes = await _context.Autopartes
            .AsNoTracking()
            .Include(a => a.IdCategoriaNavigation)
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.PrecioProductos)
            .Include(a => a.IdModelos)
            .OrderBy(a => a.IdProductoNavigation.Descripcion)
            .ToListAsync();

        return autopartes.Select(MapAutoparteDto).ToList();
    }

    public Task<bool> ExisteCodigoAsync(string codigoParte)
    {
        var codigo = codigoParte.Trim();
        return _context.Autopartes.AnyAsync(a => a.CodigoParte == codigo);
    }

    public Task<bool> ExisteAutoparteAsync(int idProducto) =>
        _context.Autopartes.AnyAsync(a => a.IdProducto == idProducto);

    public async Task<int> CrearAutoparteAsync(AutoparteCrearDto dto)
    {
        if (await ExisteCodigoAsync(dto.CodigoParte))
            throw new BusinessException("El código de parte ya existe.");

        var producto = new Producto
        {
            IdTipoProducto = 1, // Asumiendo que 1 es Autoparte
            Descripcion = dto.Descripcion.Trim(),
            Activo = true,
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var autoparte = new Autoparte
        {
            IdProducto = producto.IdProducto,
            IdCategoria = dto.IdCategoria,
            CodigoParte = dto.CodigoParte.Trim(),
            UnidadMedida = string.IsNullOrWhiteSpace(dto.UnidadMedida) ? null : dto.UnidadMedida.Trim(),
            StockActual = dto.StockActual,
            StockMinimo = dto.StockMinimo,
        };

        _context.Autopartes.Add(autoparte);

        var precioProducto = new PrecioProducto
        {
            IdProducto = producto.IdProducto,
            Precio = dto.Precio,
            FechaDesde = DateOnly.FromDateTime(DateTime.Today),
            FechaHasta = null,
        };

        _context.PrecioProductos.Add(precioProducto);
        await _context.SaveChangesAsync();

        return producto.IdProducto;
    }

    public async Task EditarAutoparteAsync(int idProducto, AutoparteEditarDto dto)
    {
        var autoparte = await _context.Autopartes
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.PrecioProductos)
            .FirstOrDefaultAsync(a => a.IdProducto == idProducto);

        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        autoparte.IdCategoria = dto.IdCategoria;
        autoparte.IdProductoNavigation.Descripcion = dto.Descripcion.Trim();

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var precioActual = autoparte.IdProductoNavigation.PrecioProductos
            .FirstOrDefault(p => p.FechaHasta == null);

        if (precioActual == null || precioActual.Precio == dto.Precio)
        {
            if (precioActual == null)
            {
                _context.PrecioProductos.Add(new PrecioProducto
                {
                    IdProducto = idProducto,
                    Precio = dto.Precio,
                    FechaDesde = hoy,
                    FechaHasta = null,
                });
            }

            await _context.SaveChangesAsync();
            return;
        }

        if (precioActual.FechaDesde >= hoy)
        {
            // El precio vigente empezo hoy: ya existe un registro con (producto, fechaDesde = hoy),
            // asi que se corrige en el lugar en vez de crear otro.
            precioActual.Precio = dto.Precio;
            await _context.SaveChangesAsync();
            return;
        }

        // Historial: se cierra el vigente y recien despues se inserta el nuevo
        // (la base permite un solo precio con fechaHasta NULL por producto).
        await using var tx = await _context.Database.BeginTransactionAsync();

        precioActual.FechaHasta = hoy.AddDays(-1);
        await _context.SaveChangesAsync();

        _context.PrecioProductos.Add(new PrecioProducto
        {
            IdProducto = idProducto,
            Precio = dto.Precio,
            FechaDesde = hoy,
            FechaHasta = null,
        });
        await _context.SaveChangesAsync();

        await tx.CommitAsync();
    }

    public async Task ActualizarStockAsync(int idProducto, int stockActual)
    {
        var autoparte = await _context.Autopartes.FirstOrDefaultAsync(a => a.IdProducto == idProducto);

        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        autoparte.StockActual = stockActual;
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAutoparteAsync(int idProducto)
    {
        var autoparte = await _context.Autopartes
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.DetalleOrdenPedidos)
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.DetalleFacturas)
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.PrecioProductos)
            .Include(a => a.IdProductoNavigation)
                .ThenInclude(p => p.Imagenes)
            .Include(a => a.ProduccionAutopartes)
            .FirstOrDefaultAsync(a => a.IdProducto == idProducto);

        if (autoparte == null)
            throw new NotFoundException("La autoparte no existe.");

        if (autoparte.IdProductoNavigation.DetalleOrdenPedidos.Any() || 
            autoparte.IdProductoNavigation.DetalleFacturas.Any())
            throw new BusinessException("No se puede eliminar una autoparte que tiene órdenes o facturas asociadas.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        // Eliminar relaciones many-to-many con modelos
        autoparte.IdModelos.Clear();

        // Eliminar datos relacionados
        _context.ProduccionAutopartes.RemoveRange(autoparte.ProduccionAutopartes);
        _context.PrecioProductos.RemoveRange(autoparte.IdProductoNavigation.PrecioProductos);
        _context.ImagenProductos.RemoveRange(autoparte.IdProductoNavigation.Imagenes);

        // Eliminar autoparte y producto
        _context.Autopartes.Remove(autoparte);
        _context.Productos.Remove(autoparte.IdProductoNavigation);

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /* ======================================================================================= */

    private static AutoparteDto MapAutoparteDto(Autoparte a)
    {
        var precio = a.IdProductoNavigation.PrecioProductos
            .Where(p => p.FechaHasta == null)
            .OrderByDescending(p => p.FechaDesde)
            .Select(p => p.Precio)
            .FirstOrDefault();

        var imagenUrl = a.IdProductoNavigation.Imagenes
            .OrderBy(i => i.Orden)
            .FirstOrDefault(i => i.EsPrincipal)
            ?.UrlImagen;

        return new AutoparteDto(
            a.IdProducto,
            a.CodigoParte,
            a.IdProductoNavigation.Descripcion,
            a.IdCategoria,
            a.IdCategoriaNavigation.Nombre,
            a.UnidadMedida ?? string.Empty,
            precio,
            a.StockActual,
            a.StockMinimo,
            imagenUrl,
            a.IdModelos.Select(m => m.IdModelo).ToList());
    }
}
