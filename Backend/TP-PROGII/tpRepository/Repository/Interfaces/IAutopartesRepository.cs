using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;
using tpRepository.Models;

namespace tpRepository.Repository.Interfaces
{
    public interface IAutopartesRepository
    {
        Task<List<AutoparteDto>> GetAutopartesAsync(bool incluirInactivos = false);
        Task<bool> ExisteCodigoAsync(string codigoParte);
        Task<bool> ExisteAutoparteAsync(int idProducto);
        Task<int> CrearAutoparteAsync(AutoparteCrearDto dto);
        Task EditarAutoparteAsync(int idProducto, AutoparteEditarDto dto);
        Task ActualizarStockAsync(int idProducto, int stockActual);
        Task EliminarAutoparteAsync(int idProducto);
    }
}
