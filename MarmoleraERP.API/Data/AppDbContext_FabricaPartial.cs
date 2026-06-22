using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Modules.Fabrica.Entities;
using MarmoleraERP.API.Modules.Fabrica.Enums;

namespace MarmoleraERP.API.Data;

/// <summary>
/// Partial de AppDbContext para registrar las entidades del módulo Fábrica.
/// Se une automáticamente al AppDbContext principal — no requiere modificar el archivo original.
/// </summary>
public partial class AppDbContext
{
    public DbSet<OrdenFabrica> OrdenesFabrica => Set<OrdenFabrica>();

    partial void OnModelCreatingFabrica(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrdenFabrica>(e =>
        {
            e.ToTable("OrdenesFabrica");
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado)
             .HasConversion<string>()
             .HasMaxLength(20);
            e.Property(x => x.OperarioId).HasMaxLength(450);
            e.Property(x => x.OperarioNombre).HasMaxLength(200);
            e.Property(x => x.Notas).HasMaxLength(1000);
        });
    }
}
