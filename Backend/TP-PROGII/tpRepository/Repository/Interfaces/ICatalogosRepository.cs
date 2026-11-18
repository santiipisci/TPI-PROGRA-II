using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tpRepository.Dtos;

namespace tpRepository.Repository.Interfaces
{
    public interface ICatalogosRepository
    {
        Task<List<TipoClienteDto>> GetTiposClienteAsync();
        Task<List<EstadoOrdenDto>> GetEstadosOrdenAsync();
        Task<List<CategoriaDto>> GetCategoriasAsync();
    }
}
