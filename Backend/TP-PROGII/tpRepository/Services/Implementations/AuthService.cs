using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Security;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio del login de administradores.</summary>
public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;

    public AuthService(IAuthRepository repo) => _repo = repo;

    public async Task<AdminDto> LoginAdminAsync(AdminLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Usuario) || string.IsNullOrWhiteSpace(dto.Clave))
            throw new UnauthorizedException("Usuario o clave incorrectos.");

        var admin = await _repo.GetAdminPorUsuarioAsync(dto.Usuario);
        if (admin == null || !PasswordHasher.Verify(dto.Clave, admin.ClaveHash))
            throw new UnauthorizedException("Usuario o clave incorrectos.");

        return new AdminDto(admin.IdUsuario, admin.Usuario, admin.Nombre);
    }

    public async Task<AdminDto> GetAdminAsync(int idUsuario)
    {
        var admin = await _repo.GetAdminPorIdAsync(idUsuario)
            ?? throw new UnauthorizedException("La sesión no es válida.");

        return new AdminDto(admin.IdUsuario, admin.Usuario, admin.Nombre);
    }
}
