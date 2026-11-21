using Microsoft.EntityFrameworkCore;

namespace tpRepository.Models;

/// <summary>
/// Configuración de las tablas que se agregaron después de generar los modelos con
/// EF Core Power Tools (USUARIO_ADMIN, SEGUIMIENTO_ORDEN, IMAGEN_PRODUCTO) y de las
/// columnas nuevas de CLIENTE y MODELO. Se mantiene en un archivo aparte para no
/// mezclarla con el código autogenerado.
/// </summary>
public partial class AutopartesDbContext
{
    public virtual DbSet<UsuarioAdmin> UsuarioAdmins { get; set; }

    public virtual DbSet<SeguimientoOrden> SeguimientoOrdens { get; set; }

    public virtual DbSet<ImagenProducto> ImagenProductos { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");

            entity.Property(e => e.ClaveHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("claveHash");

            entity.Property(e => e.FechaAlta)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime2(0)")
                .HasColumnName("fechaAlta");

            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
        });

        modelBuilder.Entity<Modelo>(entity =>
        {
            entity.Property(e => e.AnioDesde)
                .HasColumnType("smallint")
                .HasColumnName("anioDesde");

            entity.Property(e => e.AnioHasta)
                .HasColumnType("smallint")
                .HasColumnName("anioHasta");

            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
        });

        modelBuilder.Entity<UsuarioAdmin>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("USUARIO_ADMIN");

            entity.HasIndex(e => e.Usuario, "UQ_UsuarioAdmin_Usuario").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Usuario)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("usuario");
            entity.Property(e => e.ClaveHash)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("claveHash");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
        });

        modelBuilder.Entity<SeguimientoOrden>(entity =>
        {
            entity.HasKey(e => e.IdSeguimiento);

            entity.ToTable("SEGUIMIENTO_ORDEN");

            entity.HasIndex(e => new { e.IdOrdenPedido, e.Fecha }, "IX_Seguimiento_Orden");

            entity.Property(e => e.IdSeguimiento).HasColumnName("idSeguimiento");
            entity.Property(e => e.IdOrdenPedido).HasColumnName("idOrdenPedido");
            entity.Property(e => e.IdEstadoOrden).HasColumnName("idEstadoOrden");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Observacion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("observacion");

            entity.HasOne(d => d.IdOrdenPedidoNavigation)
                .WithMany(p => p.Seguimientos)
                .HasForeignKey(d => d.IdOrdenPedido)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Seguimiento_Orden");

            entity.HasOne(d => d.IdEstadoOrdenNavigation)
                .WithMany(p => p.Seguimientos)
                .HasForeignKey(d => d.IdEstadoOrden)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seguimiento_Estado");
        });

        modelBuilder.Entity<ImagenProducto>(entity =>
        {
            entity.HasKey(e => e.IdImagen);

            entity.ToTable("IMAGEN_PRODUCTO");

            entity.HasIndex(e => e.IdProducto, "IX_Imagen_Producto");

            entity.Property(e => e.IdImagen).HasColumnName("idImagen");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.UrlImagen)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("urlImagen");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsPrincipal).HasColumnName("esPrincipal");
            entity.Property(e => e.Orden).HasColumnName("orden");

            entity.HasOne(d => d.IdProductoNavigation)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Imagen_Producto");
        });

        modelBuilder.Entity<OrdenPedido>(entity =>
        {
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
        });
    }
}
