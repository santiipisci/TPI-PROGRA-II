using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Models;
using tpRepository.Dtos;

namespace tpRepository.Repository.Interfaces
{
    public interface IAuthRepository
    {
        Task<UsuarioAdmin?> GetAdminPorUsuarioAsync(string usuario);
        Task<UsuarioAdmin?> GetAdminPorIdAsync(int idUsuario);
    }
}
