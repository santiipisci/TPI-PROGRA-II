using tpRepository.Common;
using tpRepository.Dtos;
using tpRepository.Repository.Interfaces;
using tpRepository.Services.Interfaces;

namespace tpRepository.Services.Implementations;

/// <summary>Reglas de negocio de autopartes (crear, editar, eliminar, stock).</summary>
public class AutopartesService : IAutopartesService
{
    private readonly IAutopartesRepository _repo;

    public AutopartesService(IAutopartesRepository repo) => _repo = repo;

    public Task<List<AutoparteDto>> GetAutopartesAsync(bool incluirInactivos = false) => _repo.GetAutopartesAsync(incluirInactivos);

    public async Task<int> CrearAutoparteAsync(AutoparteCrearDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CodigoParte))
            throw new BusinessException("El código de parte es obligatorio.");
        if (dto.CodigoParte.Trim().Length > 50)
            throw new BusinessException("El código de parte no puede superar los 50 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            throw new BusinessException("La descripción es obligatoria.");
        if (dto.Descripcion.Trim().Length > 200)
            throw new BusinessException("La descripción no puede superar los 200 caracteres.");
        if (dto.IdCategoria <= 0)
            throw new BusinessException("La categoría es obligatoria y debe ser válida.");
        if (dto.Precio <= 0)
            throw new BusinessException("El precio debe ser mayor a 0.");
        if (dto.StockActual < 0)
            throw new BusinessException("El stock actual no puede ser negativo.");
        if (dto.StockMinimo < 0)
            throw new BusinessException("El stock mínimo no puede ser negativo.");
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl) && dto.ImagenUrl.Trim().Length > 255)
            throw new BusinessException("La URL de la imagen no puede superar los 255 caracteres.");

        return await _repo.CrearAutoparteAsync(dto);
    }

    public async Task EditarAutoparteAsync(int idProducto, AutoparteEditarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            throw new BusinessException("La descripción es obligatoria.");
        if (dto.Descripcion.Trim().Length > 200)
            throw new BusinessException("La descripción no puede superar los 200 caracteres.");
        if (dto.IdCategoria <= 0)
            throw new BusinessException("La categoría es obligatoria y debe ser válida.");
        if (dto.Precio <= 0)
            throw new BusinessException("El precio debe ser mayor a 0.");
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl) && dto.ImagenUrl.Trim().Length > 255)
            throw new BusinessException("La URL de la imagen no puede superar los 255 caracteres.");

        await _repo.EditarAutoparteAsync(idProducto, dto);
    }

    public async Task ActualizarStockAsync(int idProducto, int stockActual)
    {
        if (stockActual < 0)
            throw new BusinessException("El stock no puede ser negativo.");

        await _repo.ActualizarStockAsync(idProducto, stockActual);
    }

    public Task EliminarAutoparteAsync(int idProducto) => _repo.EliminarAutoparteAsync(idProducto);
}
