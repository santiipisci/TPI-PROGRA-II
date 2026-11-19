using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

public class ModelosService : IModelosService
{
    private readonly IModelosRepository _repository;

    public ModelosService(IModelosRepository repository)
    {
        _repository = repository;
    }

    public Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto)
    {
        if (dto.IdModelos == null || dto.IdModelos.Count == 0)
            throw new BusinessException("Selecciona al menos un modelo de vehiculo");

        if (dto.IdProductos == null || dto.IdProductos.Count == 0)
            throw new BusinessException("Selecciona al menos una autoparte");

        return _repository.ActualizarCompatibilidadLoteAsync(dto);
    }

    public Task AsociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        return _repository.AsociarModeloAutoparteAsync(idModelo, idProducto);
    }

    public async Task<int> CrearModeloAsync(ModeloCrearDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Marca))
            throw new BusinessException("La marca es obligatoria");

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new BusinessException("El nombre del modelo es obligatorio");

        if (dto.Marca.Trim().Length > 50 || dto.Nombre.Trim().Length > 50)
            throw new BusinessException("La marca y el modelo no pueden superar los 50 caracteres.");

        return await _repository.CrearModeloAsync(dto);
    }

    public Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        return _repository.DesasociarModeloAutoparteAsync(idModelo, idProducto);
    }

    public Task<List<ModeloDto>> GetModelosAsync()
    {
        return _repository.GetModelosAsync();
    }
}
