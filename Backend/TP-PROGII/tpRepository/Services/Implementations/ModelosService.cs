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
        ValidarAnios(dto.AnioDesde, dto.AnioHasta);

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
        ValidarAnios(dto.AnioDesde, dto.AnioHasta);

        await _repository.EditarModeloAsync(id, dto);
    }

    private static void ValidarAnios(short? desde, short? hasta)
    {
        if (desde.HasValue != hasta.HasValue)
            throw new BusinessException("Debe indicar el año desde y el año hasta, o dejar ambos vacíos.");
        if (desde.HasValue)
        {
            if (desde.Value < 1900 || hasta!.Value < 1900)
                throw new BusinessException("Los años deben ser mayores o iguales a 1900.");
            if (hasta.Value < desde.Value)
                throw new BusinessException("El año hasta no puede ser menor que el año desde.");
        }
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