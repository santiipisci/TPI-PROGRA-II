using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Security;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio del login de administradores.</summary>
public class AuthService : IAuthService
{
    private readonly IAuthRepository _repository;

    public AuthService(IAuthRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminDto> LoginAdminAsync(AdminLoginDto dto)
    {
        var admin = await _repository.GetAdminPorUsuarioAsync(dto.Usuario);

        if (admin == null || !PasswordHasher.Verify(dto.Clave, admin.ClaveHash))
        {
            throw new UnauthorizedException("Usuario o clave incorrecto");
        }

        return new AdminDto(admin.IdUsuario, admin.Usuario, admin.Nombre);
    }

    public async Task<AdminDto> GetAdminAsync(int idUsuario)
    {
        var admin = await _repository.GetAdminPorIdAsync(idUsuario);

        if (admin == null)
        {
            throw new UnauthorizedException("La sesión no es válida.");
        }

        return new AdminDto(admin.IdUsuario, admin.Usuario, admin.Nombre);
    }
}
