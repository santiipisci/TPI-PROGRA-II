using System.Text.RegularExpressions;
using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Security;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de clientes. Los controladores solo traducen HTTP ↔ servicio.</summary>
public class ClienteService : IClienteService
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> RegistrarClienteAsync(ClienteRegistroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreRazonSocial))
            throw new BusinessException("El nombre o razón social es obligatorio.");
        if (dto.NombreRazonSocial.Trim().Length > 100)
            throw new BusinessException("El nombre no puede superar los 100 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.DocumentoCuit))
            throw new BusinessException("El documento o CUIT es obligatorio.");
        if (dto.DocumentoCuit.Trim().Length > 13)
            throw new BusinessException("El documento o CUIT no puede superar los 13 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email.Trim()))
            throw new BusinessException("El email no tiene un formato válido.");
        if (string.IsNullOrWhiteSpace(dto.Clave) || dto.Clave.Length < 6)
            throw new BusinessException("La clave debe tener al menos 6 caracteres.");

        var limpio = dto with
        {
            NombreRazonSocial = dto.NombreRazonSocial.Trim(),
            DocumentoCuit = dto.DocumentoCuit.Trim(),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
            Email = dto.Email.Trim().ToLower(),
        };

        if (await _repository.GetClientePorEmailAsync(limpio.Email) != null)
            throw new DuplicateException("Ya existe una cuenta con ese email.");
        if (await _repository.GetClientePorDocumentoAsync(limpio.DocumentoCuit) != null)
            throw new DuplicateException("Ya existe un cliente con ese documento o CUIT.");

        return await _repository.RegistrarClienteAsync(dto, PasswordHasher.Hash(dto.Clave));
    }

    public async Task<ClienteDto> LoginClienteAsync(ClienteLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Clave))
            throw new UnauthorizedException("Email o clave incorrectos.");

        var cliente = await _repository.GetClientePorEmailAsync(dto.Email);
        if (cliente == null || !PasswordHasher.Verify(dto.Clave, cliente.ClaveHash))
            throw new UnauthorizedException("Email o clave incorrectos.");

        return await _repository.GetClienteAsync(cliente.IdCliente)
            ?? throw new UnauthorizedException("Email o clave incorrectos.");
    }

    public async Task<ClienteDto?> GetClienteAsync(int idCliente)
    {
        return await _repository.GetClienteAsync(idCliente);
    }

    public async Task<List<ClienteDto>> GetClientesAsync()
    {
        return await _repository.GetClientesAsync();
    }
}
