namespace tpRepository.Models;

public partial class UsuarioAdmin
{
    public int IdUsuario { get; set; }

    public string Usuario { get; set; } = null!;

    public string ClaveHash { get; set; } = null!;

    public string? Nombre { get; set; }

    public bool Activo { get; set; }
}
