using tpRepository.Dtos;

namespace tpRepository.Services.Interfaces;

public interface IClienteService
{
    Task<int> RegistrarClienteAsync(ClienteRegistroDto dto);
    Task<ClienteDto> LoginClienteAsync(ClienteLoginDto dto);
    Task<ClienteDto?> GetClienteAsync(int idCliente);
    Task<List<ClienteDto>> GetClientesAsync();
}
