using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;
using tpRepository.Models;

namespace tpRepository.Repository.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> GetClientePorEmailAsync(string email);
        Task<Cliente?> GetClientePorDocumentoAsync(string documento);
        Task<ClienteDto?> GetClienteAsync(int idCliente);
        Task<List<ClienteDto>> GetClientesAsync();
        Task<int> RegistrarClienteAsync(ClienteRegistroDto dto, string claveHash);
    }
}
