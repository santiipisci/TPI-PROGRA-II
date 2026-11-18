namespace tpRepository.Models;

/// <summary>Evento del seguimiento de una orden de pedido (tabla SEGUIMIENTO_ORDEN).</summary>
public partial class SeguimientoOrden
{
    public int IdSeguimiento { get; set; }

    public int IdOrdenPedido { get; set; }

    public int IdEstadoOrden { get; set; }

    public DateOnly Fecha { get; set; }

    public string? Observacion { get; set; }

    public virtual OrdenPedido IdOrdenPedidoNavigation { get; set; } = null!;

    public virtual EstadoOrden IdEstadoOrdenNavigation { get; set; } = null!;
}
