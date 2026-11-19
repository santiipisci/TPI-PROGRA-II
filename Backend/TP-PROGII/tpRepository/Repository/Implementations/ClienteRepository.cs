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
        return await _context.Clientes
                     .Where(c => c.IdCliente == idCliente)
                     .Select(c => new ClienteDto(
                        c.IdCliente,
                        c.IdTipoCliente,
                        c.NombreRazonSocial,
                        c.DocumentoCuit,
                        c.Telefono,
                        c.Direccion,
                        c.Email,
                        c.IdTipoClienteNavigation.Descripcion))
                     .FirstOrDefaultAsync();
    }

    public async Task<List<ClienteDto>> GetClientesAsync()
    {
        return await _context.Clientes
                     .OrderBy(c => c.NombreRazonSocial)
                     .Select(c => new ClienteDto(
                        c.IdCliente,
                        c.IdTipoCliente,
                        c.NombreRazonSocial,
                        c.DocumentoCuit,
                        c.Telefono,
                        c.Direccion,
                        c.Email,
                        c.IdTipoClienteNavigation.Descripcion))
                     .ToListAsync();
    }

    public async Task<int> RegistrarClienteAsync(ClienteRegistroDto dto, string claveHash)
    {
        if (!await _context.TipoClientes.AnyAsync(t => t.IdTipoCliente == dto.IdTipoCliente))
            throw new BusinessException("El tipo de cliente indicado no existe.");

        var cliente = new Cliente
        {
            IdTipoCliente = dto.IdTipoCliente,
            NombreRazonSocial = dto.NombreRazonSocial,
            DocumentoCuit = dto.DocumentoCuit,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Email = dto.Email,
            ClaveHash = claveHash,
            FechaAlta = DateTime.Now,
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return cliente.IdCliente;
    }
}
