namespace tpRepository.Dtos;

/* =====================================================================
   DTOs que viajan entre la API y el front (JSON camelCase por defecto).
   Los nombres de los campos coinciden con lo que espera el contrato
   documentado en Frontend/AutopartesFront/autopartes/API_CONTRATO.md.
   ===================================================================== */

public record AutoparteDto(
    int IdProducto,
    string CodigoParte,
    string Descripcion,
    int IdCategoria,
    string Categoria,
    string UnidadMedida,
    decimal Precio,
    int StockActual,
    int StockMinimo,
    string? ImagenUrl,
    List<int> Modelos);

public record ModeloDto(
    int IdModelo,
    int IdMarca,
    string Marca,
    string Nombre,
    short? AnioDesde,
    short? AnioHasta);

public record CategoriaDto(int IdCategoria, string Nombre);

public record TipoClienteDto(int IdTipoCliente, string Descripcion);

public record EstadoOrdenDto(int IdEstadoOrden, string Descripcion);

public record ClienteDto(
    int IdCliente,
    int IdTipoCliente,
    string NombreRazonSocial,
    string DocumentoCuit,
    string? Telefono,
    string? Direccion,
    string Email,
    string TipoCliente);

public record ClienteRegistroDto(
    int IdTipoCliente,
    string NombreRazonSocial,
    string DocumentoCuit,
    string? Telefono,
    string? Direccion,
    string Email,
    string Clave);

public record ClienteLoginDto(string Email, string Clave);

public record AdminLoginDto(string Usuario, string Clave);

public record AdminDto(int IdUsuario, string Usuario, string? Nombre);

public record AutoparteCrearDto(
    string CodigoParte,
    string Descripcion,
    int IdCategoria,
    string? UnidadMedida,
    decimal Precio,
    int StockActual,
    int StockMinimo);

public record AutoparteEditarDto(string Descripcion, int IdCategoria, decimal Precio);

public record StockDto(int StockActual);

public record ModeloCrearDto(string Marca, string Nombre);

public record VentaMesDto(int Anio, int Mes, decimal Total);

public record OrdenEstadoCantidadDto(string Estado, int Cantidad);

public record ResumenDto(
    decimal VentasUltimoMes,
    decimal VentasUltimoAnio,
    int OrdenesTotales,
    List<VentaMesDto> VentasPorMes,
    List<OrdenEstadoCantidadDto> OrdenesPorEstado);

public record OrdenListadoDto(
    int IdOrdenPedido,
    DateOnly FechaPedido,
    DateOnly? FechaEntrega,
    string Cliente,
    string TipoCliente,
    string Estado,
    int IdEstadoOrden,
    decimal Total);

public record OrdenPaginadaDto(List<OrdenListadoDto> Items, int TotalRegistros, decimal SumaTotal);

public record OrdenItemDto(int IdProducto, string Descripcion, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record SeguimientoDto(int IdSeguimiento, DateOnly Fecha, int IdEstadoOrden, string Estado, string? Observacion);

public record OrdenDetalleDto(
    int IdOrdenPedido,
    int IdCliente,
    string Cliente,
    int IdEstadoOrden,
    string Estado,
    DateOnly FechaPedido,
    DateOnly? FechaEntrega,
    decimal Total,
    List<OrdenItemDto> Items,
    List<SeguimientoDto> Seguimiento);

public record OrdenItemGuardarDto(int IdProducto, int Cantidad);

public record OrdenGuardarDto(
    int IdCliente,
    int IdEstadoOrden,
    DateOnly FechaPedido,
    DateOnly? FechaEntrega,
    List<OrdenItemGuardarDto>? Items);

public record SeguimientoCrearDto(DateOnly Fecha, int IdEstadoOrden, string? Observacion);

/// <summary>
/// Actualización masiva de compatibilidad autoparte ↔ modelo. Permite asociar o
/// quitar varias combinaciones en una sola operación (pensado para catálogos grandes).
/// </summary>
public record CompatibilidadLoteDto(List<int>? IdModelos, List<int>? IdProductos, bool Asociar);
