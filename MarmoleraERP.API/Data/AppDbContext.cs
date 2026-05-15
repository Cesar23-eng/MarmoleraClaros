using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Modules.Identity.Entities;
using MarmoleraERP.API.Modules.Ventas.Entities;
using MarmoleraERP.API.Modules.Catalogo.Entities;
using MarmoleraERP.API.Modules.Ventas.Enums;
using MarmoleraERP.API.Modules.Catalogo.Enums;

namespace MarmoleraERP.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ─── DbSets ──────────────────────────────────────────────────────────────
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<ServicioExtra> ServiciosExtras => Set<ServicioExtra>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedidoGeometria> DetallesGeometria => Set<DetallePedidoGeometria>();
    public DbSet<DetallePedidoExtra> DetallesExtras => Set<DetallePedidoExtra>();
    public DbSet<Cotizacion>       Cotizaciones     => Set<Cotizacion>();
    public DbSet<DetalleCotizacion> DetallesCotizacion => Set<DetalleCotizacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Requerido para Identity

        // ─── Cliente ─────────────────────────────────────────────────────────
        builder.Entity<Cliente>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.NombreCompleto).IsRequired().HasMaxLength(150);
            e.Property(c => c.Telefono).HasMaxLength(20);
            e.Property(c => c.Direccion).HasMaxLength(300);
            e.Property(c => c.Nit_Ci).HasMaxLength(30);
        });

        // ─── Material ────────────────────────────────────────────────────────
        builder.Entity<Material>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
            e.Property(m => m.Categoria).HasMaxLength(80);
            e.Property(m => m.PrecioPorM2).HasPrecision(18, 4);
        });

        // ─── ServicioExtra ───────────────────────────────────────────────────
        builder.Entity<ServicioExtra>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Descripcion).IsRequired().HasMaxLength(200);
            e.Property(s => s.TipoCobro).HasConversion<string>();
            e.Property(s => s.Precio).HasPrecision(18, 4);
        });

        // ─── Pedido ──────────────────────────────────────────────────────────
        builder.Entity<Pedido>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Estado)
             .HasConversion<string>()   // Guarda el enum como texto legible
             .HasMaxLength(30);
            e.Property(p => p.TotalFinal).HasPrecision(18, 4);

            e.HasOne(p => p.Cliente)
             .WithMany(c => c.Pedidos)
             .HasForeignKey(p => p.ClienteId)
             .OnDelete(DeleteBehavior.Restrict); // No eliminar cliente con pedidos
        });

        // ─── DetallePedidoGeometria ───────────────────────────────────────────
        builder.Entity<DetallePedidoGeometria>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.PlantillaUsada).HasConversion<string>().HasMaxLength(20);
            e.Property(d => d.LadoA).HasPrecision(18, 4);
            e.Property(d => d.LadoB).HasPrecision(18, 4);
            e.Property(d => d.LadoC).HasPrecision(18, 4);
            e.Property(d => d.Ancho).HasPrecision(18, 4);
            e.Property(d => d.AreaCalculadaM2).HasPrecision(18, 4);
            e.Property(d => d.SubtotalMaterial).HasPrecision(18, 4);

            e.HasOne(d => d.Pedido)
             .WithMany(p => p.Geometrias)
             .HasForeignKey(d => d.PedidoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Material)
             .WithMany()
             .HasForeignKey(d => d.MaterialId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── DetallePedidoExtra ───────────────────────────────────────────────
        builder.Entity<DetallePedidoExtra>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Cantidad).HasPrecision(18, 4);
            e.Property(d => d.SubtotalExtra).HasPrecision(18, 4);

            e.HasOne(d => d.DetalleGeometria)
             .WithMany(g => g.ServiciosExtras)
             .HasForeignKey(d => d.DetalleGeometriaId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.ServicioExtra)
             .WithMany()
             .HasForeignKey(d => d.ServicioExtraId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Seed Roles ──────────────────────────────────────────────────────
        SeedRoles(builder);

        // ─── Cotizacion (cabecera) ────────────────────────────────────────────
        builder.Entity<Cotizacion>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.UsuarioId).IsRequired().HasMaxLength(450);
            e.Property(c => c.PrecioTotal).HasPrecision(18, 4);
            e.Property(c => c.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Cotizado");
            e.Property(c => c.Comentarios).HasMaxLength(500);
            e.Property(c => c.FechaAprobacion).IsRequired(false); // NULL hasta que sea aprobada

            e.HasOne(c => c.Cliente)
             .WithMany()
             .HasForeignKey(c => c.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── DetalleCotizacion ────────────────────────────────────────────────
        builder.Entity<DetalleCotizacion>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.NombreMaterial).IsRequired().HasMaxLength(150);
            e.Property(d => d.Geometria).HasConversion<string>().HasMaxLength(20);
            e.Property(d => d.MedidasJson).IsRequired().HasMaxLength(500);
            e.Property(d => d.PrecioPorM2).HasPrecision(18, 4);
            e.Property(d => d.AreaTotal).HasPrecision(18, 4);
            e.Property(d => d.PrecioSubtotal).HasPrecision(18, 4);

            e.HasOne(d => d.Cotizacion)
             .WithMany(c => c.Detalles)
             .HasForeignKey(d => d.CotizacionId)
             .OnDelete(DeleteBehavior.Cascade); // Al eliminar la cabecera se eliminan sus detalles
        });
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        var roles = new[]
        {
            new IdentityRole { Id = "1", Name = "Admin",        NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "Ventas",       NormalizedName = "VENTAS" },
            new IdentityRole { Id = "3", Name = "Produccion",   NormalizedName = "PRODUCCION" },
            new IdentityRole { Id = "4", Name = "Contabilidad", NormalizedName = "CONTABILIDAD" },
        };

        builder.Entity<IdentityRole>().HasData(roles);
    }
}
