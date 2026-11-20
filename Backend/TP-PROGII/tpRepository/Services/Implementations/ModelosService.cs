using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de modelos y compatibilidad con autopartes.</summary>
public class ModelosService : IModelosService
{
    private readonly IModelosRepository _repo;

    public ModelosService(IModelosRepository repo) => _repo = repo;

    public Task<List<ModeloDto>> GetModelosAsync() => _repo.GetModelosAsync();

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

        return await _repo.CrearModeloAsync(dto);
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

        await _repo.EditarModeloAsync(id, dto);
    }

    public Task EliminarModeloAsync(int id) =>
        _repo.EliminarModeloAsync(id);

    public Task AsociarModeloAutoparteAsync(int idModelo, int idProducto) =>
        _repo.AsociarModeloAutoparteAsync(idModelo, idProducto);

    public Task DesasociarModeloAutoparteAsync(int idModelo, int idProducto) =>
        _repo.DesasociarModeloAutoparteAsync(idModelo, idProducto);

    public Task<int> ActualizarCompatibilidadLoteAsync(CompatibilidadLoteDto dto) =>
        _repo.ActualizarCompatibilidadLoteAsync(dto);
}
