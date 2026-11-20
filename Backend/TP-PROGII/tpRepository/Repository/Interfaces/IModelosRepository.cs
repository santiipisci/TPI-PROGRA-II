using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;
using tpRepository.Models;

namespace tpRepository.Repository.Interfaces
{
    public interface IModelosRepository
    {
        Task<List<ModeloDto>> GetModelosAsync();
        Task<bool> ExisteModeloAsync(string marca, string nombre);
        Task<int> CrearModeloAsync(ModeloCrearDto dto);
        Task EditarModeloAsync(int id, ModeloEditarDto dto);
        Task EliminarModeloAsync(int id);
        Task AsociarModeloAutoparteAsync(int idModelo, int idProducto);
        Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto);
        Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto);
    }
}
