using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;
using tpRepository.Models;
using tpRepository.Common;
using tpRepository.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace tpRepository.Repository.Implementations
{
    public class OrdenesRepository : IOrdenesRepository
    {
        private readonly AutopartesDbContext _context;
        private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

        public OrdenesRepository(AutopartesDbContext context)
        {
            _context = context;
        }

        public async Task<SeguimientoDto> AgregarSeguimientoAsync(int idOrden, SeguimientoCrearDto dto)
        {
            var orden = await _context.OrdenPedidos.FirstOrDefaultAsync(o => o.IdOrdenPedido == idOrden)
                ?? throw new NotFoundException("La orden no existe.");

            var estado = await _context.EstadoOrdens.FirstOrDefaultAsync(e => e.IdEstadoOrden == dto.IdEstadoOrden)
                ?? throw new BusinessException("El estado indicado no existe.");

            var seguimiento = new SeguimientoOrden
            {
                IdOrdenPedido = idOrden,
                IdEstadoOrden = dto.IdEstadoOrden,
                Fecha = dto.Fecha,
                Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
            };

            _context.SeguimientoOrdens.Add(seguimiento);

            // El estado actual de la orden siempre refleja el último seguimiento.
            orden.IdEstadoOrden = dto.IdEstadoOrden;
            if (estado.Descripcion.Equals("Entregada", StringComparison.OrdinalIgnoreCase) && orden.FechaEntrega == null)
                orden.FechaEntrega = dto.Fecha;

            await _context.SaveChangesAsync();

            return new SeguimientoDto(
                seguimiento.IdSeguimiento,
                seguimiento.Fecha,
                estado.IdEstadoOrden,
                estado.Descripcion,
                seguimiento.Observacion);
        }

        public async Task<int> CrearOrdenAsync(OrdenGuardarDto dto)
        {
            await ValidarOrdenAsync(dto);

            var items = (dto.Items ?? new List<OrdenItemGuardarDto>())
                .GroupBy(i => i.IdProducto)
                .Select(g => new OrdenItemGuardarDto(g.Key, g.Sum(x => x.Cantidad)))
                .ToList();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var orden = new OrdenPedido
            {
                IdCliente = dto.IdCliente,
                IdEstadoOrden = dto.IdEstadoOrden,
                FechaPedido = dto.FechaPedido,
                FechaEntrega = dto.FechaEntrega,
            };

            _context.OrdenPedidos.Add(orden);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                _context.DetalleOrdenPedidos.Add(new DetalleOrdenPedido
                {
                    IdOrdenPedido = orden.IdOrdenPedido,
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                });
            }

            // Primer evento del seguimiento: el alta de la orden.
            _context.SeguimientoOrdens.Add(new SeguimientoOrden
            {
                IdOrdenPedido = orden.IdOrdenPedido,
                IdEstadoOrden = dto.IdEstadoOrden,
                Fecha = dto.FechaPedido,
                Observacion = "Alta de la orden",
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return orden.IdOrdenPedido;
        }

        public async Task<bool> EditarOrdenAsync(int idOrden, OrdenGuardarDto dto)
        {
            await ValidarOrdenAsync(dto);

            var orden = await _context.OrdenPedidos
                .Include(o => o.DetalleOrdenPedidos)
                .FirstOrDefaultAsync(o => o.IdOrdenPedido == idOrden);

            if (orden == null) return false;

            var items = (dto.Items ?? new List<OrdenItemGuardarDto>())
                .GroupBy(i => i.IdProducto)
                .Select(g => new OrdenItemGuardarDto(g.Key, g.Sum(x => x.Cantidad)))
                .ToList();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var cambioEstado = orden.IdEstadoOrden != dto.IdEstadoOrden;
            orden.IdCliente = dto.IdCliente;
            orden.IdEstadoOrden = dto.IdEstadoOrden;
            orden.FechaPedido = dto.FechaPedido;
            orden.FechaEntrega = dto.FechaEntrega;

            _context.DetalleOrdenPedidos.RemoveRange(orden.DetalleOrdenPedidos);
            orden.DetalleOrdenPedidos.Clear();

            foreach (var item in items)
            {
                orden.DetalleOrdenPedidos.Add(new DetalleOrdenPedido
                {
                    IdOrdenPedido = orden.IdOrdenPedido,
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                });
            }

            if (cambioEstado)
            {
                _context.SeguimientoOrdens.Add(new SeguimientoOrden
                {
                    IdOrdenPedido = orden.IdOrdenPedido,
                    IdEstadoOrden = dto.IdEstadoOrden,
                    Fecha = Hoy,
                    Observacion = "Cambio de estado desde la edición de la orden",
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<bool> EliminarOrdenAsync(int idOrden)
        {
            var orden = await _context.OrdenPedidos
                .Include(o => o.DetalleOrdenPedidos)
                .Include(o => o.Seguimientos)
                .FirstOrDefaultAsync(o => o.IdOrdenPedido == idOrden);

            if (orden == null) return false;

            if (await _context.Facturas.AnyAsync(f => f.IdOrdenPedido == idOrden))
                throw new BusinessException("No se puede eliminar una orden que ya tiene factura asociada.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            _context.SeguimientoOrdens.RemoveRange(orden.Seguimientos);
            _context.DetalleOrdenPedidos.RemoveRange(orden.DetalleOrdenPedidos);
            _context.OrdenPedidos.Remove(orden);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<bool> EliminarSeguimientoAsync(int idSeguimiento)
        {
            var seguimiento = await _context.SeguimientoOrdens.FirstOrDefaultAsync(s => s.IdSeguimiento == idSeguimiento);
            if (seguimiento == null) return false;

            _context.SeguimientoOrdens.Remove(seguimiento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrdenDetalleDto?> GetOrdenAsync(int idOrden)
        {
            var o = await _context.OrdenPedidos
                .AsNoTracking()
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdEstadoOrdenNavigation)
                .Include(x => x.DetalleOrdenPedidos).ThenInclude(d => d.IdProductoNavigation).ThenInclude(p => p.PrecioProductos)
                .Include(x => x.Seguimientos).ThenInclude(s => s.IdEstadoOrdenNavigation)
                .FirstOrDefaultAsync(x => x.IdOrdenPedido == idOrden);

            if (o == null) return null;

            var items = o.DetalleOrdenPedidos
                .OrderBy(d => d.IdProductoNavigation.Descripcion)
                .Select(d =>
                {
                    var precio = d.IdProductoNavigation.PrecioProductos
                        .Where(p => p.FechaHasta == null)
                        .OrderByDescending(p => p.FechaDesde)
                        .Select(p => p.Precio)
                        .FirstOrDefault();

                    return new OrdenItemDto(
                        d.IdProducto,
                        d.IdProductoNavigation.Descripcion,
                        d.Cantidad,
                        precio,
                        precio * d.Cantidad);
                })
                .ToList();

            var seguimiento = o.Seguimientos
                .OrderBy(s => s.Fecha)
                .ThenBy(s => s.IdSeguimiento)
                .Select(s => new SeguimientoDto(
                    s.IdSeguimiento,
                    s.Fecha,
                    s.IdEstadoOrden,
                    s.IdEstadoOrdenNavigation.Descripcion,
                    s.Observacion))
                .ToList();

            return new OrdenDetalleDto(
                o.IdOrdenPedido,
                o.IdCliente,
                o.IdClienteNavigation.NombreRazonSocial,
                o.IdEstadoOrden,
                o.IdEstadoOrdenNavigation.Descripcion,
                o.FechaPedido,
                o.FechaEntrega,
                items.Sum(i => i.Subtotal),
                items,
                seguimiento);
        }

        public async Task<OrdenPaginadaDto> GetOrdenesAsync(int? idEstadoOrden, string? q, int pagina, int tamano)
        {
            pagina = Math.Max(1, pagina);
            tamano = Math.Clamp(tamano, 1, 100);

            var query = _context.OrdenPedidos.AsNoTracking().AsQueryable();

            if (idEstadoOrden is > 0)
                query = query.Where(o => o.IdEstadoOrden == idEstadoOrden);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var texto = q.Trim();
                var numero = int.TryParse(texto.TrimStart('#'), out var nro);
                query = query.Where(o =>
                    o.IdClienteNavigation.NombreRazonSocial.Contains(texto) || (numero && o.IdOrdenPedido == nro));
            }

            var totalRegistros = await query.CountAsync();
            var ids = await query.Select(o => o.IdOrdenPedido).ToListAsync();
            var totales = await GetTotalesAsync(ids);
            var sumaTotal = totales.Values.Sum();

            var page = await query
                .OrderByDescending(o => o.FechaPedido)
                .ThenByDescending(o => o.IdOrdenPedido)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Select(o => new
                {
                    o.IdOrdenPedido,
                    o.FechaPedido,
                    o.FechaEntrega,
                    o.IdEstadoOrden,
                    Cliente = o.IdClienteNavigation.NombreRazonSocial,
                    TipoCliente = o.IdClienteNavigation.IdTipoClienteNavigation.Descripcion,
                    Estado = o.IdEstadoOrdenNavigation.Descripcion,
                })
                .ToListAsync();

            var items = page.Select(o => new OrdenListadoDto(
                o.IdOrdenPedido,
                o.FechaPedido,
                o.FechaEntrega,
                o.Cliente,
                o.TipoCliente,
                o.Estado,
                o.IdEstadoOrden,
                totales.GetValueOrDefault(o.IdOrdenPedido))).ToList();

            return new OrdenPaginadaDto(items, totalRegistros, sumaTotal);
        }

        public async Task<ResumenDto> GetResumenAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
            var inicioAnio = new DateOnly(hoy.Year, 1, 1);
            var inicioVentana = inicioMes.AddMonths(-11);

            var ordenes = await _context.OrdenPedidos
                .AsNoTracking()
                .Select(o => new { o.IdOrdenPedido, o.IdEstadoOrden, o.FechaPedido })
                .ToListAsync();

            var totales = await GetTotalesAsync(ordenes.Select(o => o.IdOrdenPedido));

            var estados = await _context.EstadoOrdens.AsNoTracking().OrderBy(e => e.IdEstadoOrden).ToListAsync();

            var ventasPorMes = ordenes
                .Where(o => o.FechaPedido >= inicioVentana)
                .GroupBy(o => new { o.FechaPedido.Year, o.FechaPedido.Month })
                .Select(g => new VentaMesDto(g.Key.Year, g.Key.Month, g.Sum(o => totales.GetValueOrDefault(o.IdOrdenPedido))))
                .OrderBy(v => v.Anio).ThenBy(v => v.Mes)
                .ToList();

            return new ResumenDto(
                ordenes.Where(o => o.FechaPedido >= inicioMes).Sum(o => totales.GetValueOrDefault(o.IdOrdenPedido)),
                ordenes.Where(o => o.FechaPedido >= inicioAnio).Sum(o => totales.GetValueOrDefault(o.IdOrdenPedido)),
                ordenes.Count,
                ventasPorMes,
                estados.Select(e => new OrdenEstadoCantidadDto(
                    e.Descripcion,
                    ordenes.Count(o => o.IdEstadoOrden == e.IdEstadoOrden))).ToList());
        }

        /* ================================================================================== */

        private async Task ValidarOrdenAsync(OrdenGuardarDto dto)
        {
            if (!await _context.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
                throw new BusinessException("El cliente indicado no existe.");

            if (!await _context.EstadoOrdens.AnyAsync(e => e.IdEstadoOrden == dto.IdEstadoOrden))
                throw new BusinessException("El estado indicado no existe.");

            if (dto.FechaEntrega is { } entrega && entrega < dto.FechaPedido)
                throw new BusinessException("La fecha de entrega no puede ser anterior a la fecha del pedido.");

            var items = dto.Items ?? new List<OrdenItemGuardarDto>();
            if (items.Any(i => i.Cantidad <= 0))
                throw new BusinessException("Las cantidades del detalle deben ser mayores a 0.");

            var ids = items.Select(i => i.IdProducto).Distinct().ToList();
            if (ids.Count == 0) return;

            var existentes = await _context.Autopartes.CountAsync(a => ids.Contains(a.IdProducto));
            if (existentes != ids.Count)
                throw new BusinessException("Alguna de las autopartes del detalle no existe.");
        }

        private async Task<Dictionary<int, decimal>> GetTotalesAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            if (lista.Count == 0) return new Dictionary<int, decimal>();

            var detalles = await _context.DetalleOrdenPedidos
                .AsNoTracking()
                .Where(d => lista.Contains(d.IdOrdenPedido))
                .Select(d => new
                {
                    d.IdOrdenPedido,
                    d.Cantidad,
                    Precio = d.IdProductoNavigation.PrecioProductos
                        .Where(p => p.FechaHasta == null)
                        .Select(p => (decimal?)p.Precio)
                        .FirstOrDefault() ?? 0m,
                })
                .ToListAsync();

            return detalles
                .GroupBy(d => d.IdOrdenPedido)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad * x.Precio));
        }
    }
}
