using Microsoft.EntityFrameworkCore;
using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Models;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Repository.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly AutopartesDbContext _context;

    public ClienteRepository(AutopartesDbContext context)
    {
        _context = context;
    }

    public Task<Cliente?> GetClientePorEmailAsync(string email)
    {
        var normalizado = email.Trim().ToLower();
        return _context.Clientes.FirstOrDefaultAsync(c => c.Email.ToLower() == normalizado);
    }

    public Task<Cliente?> GetClientePorDocumentoAsync(string documento)
    {
        var doc = documento.Trim();
        return _context.Clientes.FirstOrDefaultAsync(c => c.DocumentoCuit == doc);
    }

    public async Task<ClienteDto?> GetClienteAsync(int idCliente)
    {
        var c = await _context.Clientes
            .AsNoTracking()
            .Include(x => x.IdTipoClienteNavigation)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente);

        return c == null ? null : MapCliente(c);
    }

    public async Task<List<ClienteDto>> GetClientesAsync()
    {
        var data = await _context.Clientes
            .AsNoTracking()
            .Include(c => c.IdTipoClienteNavigation)
            .OrderBy(c => c.NombreRazonSocial)
            .ToListAsync();

        return data.Select(MapCliente).ToList();
    }

    public async Task<int> RegistrarClienteAsync(ClienteRegistroDto dto, string claveHash)
    {
        // Validación del tipo de cliente (antes la hacía el servicio con el catálogo).
        if (!await _context.TipoClientes.AnyAsync(t => t.IdTipoCliente == dto.IdTipoCliente))
            throw new BusinessException("El tipo de cliente indicado no existe.");

        var cliente = new Cliente
        {
            IdTipoCliente = dto.IdTipoCliente,
            NombreRazonSocial = dto.NombreRazonSocial.Trim(),
            DocumentoCuit = dto.DocumentoCuit.Trim(),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
            Email = dto.Email.Trim().ToLower(),
            ClaveHash = claveHash,
            FechaAlta = DateTime.Now,
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return cliente.IdCliente;
    }

    /* ======================================================================================= */

    private static ClienteDto MapCliente(Cliente c) => new(
        c.IdCliente,
        c.IdTipoCliente,
        c.NombreRazonSocial,
        c.DocumentoCuit,
        c.Telefono,
        c.Direccion,
        c.Email,
        c.IdTipoClienteNavigation?.Descripcion ?? string.Empty);
}
