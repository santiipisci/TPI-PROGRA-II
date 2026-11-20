using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de modelos y compatibilidad con autopartes.</summary>
public class ModelosService : IModelosService
{
    private readonly IModelosRepository _repository;

    public ModelosService(IModelosRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ModeloDto>> GetModelosAsync()
    {
        return await _repository.GetModelosAsync();
    }


    public async Task<int> CrearModeloAsync(ModeloCrearDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Marca))
            throw new BusinessException("La marca es obligatoria.");
        if (dto.Marca.Trim().Length > 50)
            throw new BusinessException("La marca no puede superar los 50 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new BusinessException("El nombre del modelo es obligatorio.");
        if (dto.Nombre.Trim().Length > 50)
            throw new BusinessException("El nombre del modelo no puede superar los 50 caracteres.");

        return await _repository.CrearModeloAsync(dto);
    }

    public async Task EditarModeloAsync(int id, ModeloEditarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Marca))
            throw new BusinessException("La marca es obligatoria.");
        if (dto.Marca.Trim().Length > 50)
            throw new BusinessException("La marca no puede superar los 50 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new BusinessException("El nombre del modelo es obligatorio.");
        if (dto.Nombre.Trim().Length > 50)
            throw new BusinessException("El nombre del modelo no puede superar los 50 caracteres.");

        await _repository.EditarModeloAsync(id, dto);
    }

    public async Task EliminarModeloAsync(int id)
    {
        await _repository.EliminarModeloAsync(id);
    }

    public async Task AsociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        await _repository.AsociarModeloAutoparteAsync(idModelo, idProducto);
    }

    public async Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto)
    {
        await _repository.DesasociarModeloAutoparteAsync(idModelo, idProducto);
    }

    public async Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto)
    {
        if (dto.IdModelos == null || dto.IdModelos.Count == 0)
            throw new BusinessException("Seleccioná al menos un modelo de vehículo.");
        if (dto.IdProductos == null || dto.IdProductos.Count == 0)
            throw new BusinessException("Seleccioná al menos una autoparte.");

        return await _repository.ActualizarCompatibilidadLoteAsync(dto);
    }
}