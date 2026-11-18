namespace tpRepository.Models;

public partial class ImagenProducto
{
    public int IdImagen { get; set; }

    public int IdProducto { get; set; }

    public string UrlImagen { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool EsPrincipal { get; set; }

    public byte Orden { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
