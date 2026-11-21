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
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TelefonoRegex = new(@"^\d{7,15}$", RegexOptions.Compiled);

    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

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
        if (!string.IsNullOrWhiteSpace(dto.Telefono) && !TelefonoRegex.IsMatch(dto.Telefono.Trim()))
            throw new BusinessException("El teléfono debe contener solo números (entre 7 y 15 dígitos).");
        if (string.IsNullOrWhiteSpace(dto.Clave) || dto.Clave.Length < 6)
            throw new BusinessException("La clave debe tener al menos 6 caracteres.");

        if (await _repo.GetClientePorEmailAsync(dto.Email) != null)
            throw new DuplicateException("Ya existe una cuenta con ese email.");
        if (await _repo.GetClientePorDocumentoAsync(dto.DocumentoCuit) != null)
            throw new DuplicateException("Ya existe un cliente con ese documento o CUIT.");

        return await _repo.RegistrarClienteAsync(dto, PasswordHasher.Hash(dto.Clave));
    }

    public async Task<ClienteDto> LoginClienteAsync(ClienteLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Clave))
            throw new UnauthorizedException("Email o clave incorrectos.");

        var cliente = await _repo.GetClientePorEmailAsync(dto.Email);
        if (cliente == null || string.IsNullOrWhiteSpace(cliente.ClaveHash) || !PasswordHasher.Verify(dto.Clave, cliente.ClaveHash))
            throw new UnauthorizedException("Email o clave incorrectos.");

        return await _repo.GetClienteAsync(cliente.IdCliente)
            ?? throw new UnauthorizedException("Email o clave incorrectos.");
    }

    public Task<ClienteDto?> GetClienteAsync(int idCliente) => _repo.GetClienteAsync(idCliente);

    public Task<List<ClienteDto>> GetClientesAsync() => _repo.GetClientesAsync();
}
