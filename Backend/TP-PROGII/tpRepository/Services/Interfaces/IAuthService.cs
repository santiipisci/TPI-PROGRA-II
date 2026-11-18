using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IAuthService
{
    Task<AdminDto> LoginAdminAsync(AdminLoginDto dto);
    Task<AdminDto> GetAdminAsync(int idUsuario);
}
