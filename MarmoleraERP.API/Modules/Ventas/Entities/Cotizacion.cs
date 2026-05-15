namespace MarmoleraERP.API.Modules.Ventas.Entities;

/// <summary>
/// Cabecera de cotización. Puede contener uno o varios mesones (DetalleCotizacion).
/// Es el paso previo a un Pedido formal.
/// Ciclo de vida: "Cotizado" → "Aprobado" → "EnProduccion" → "Finalizado".
/// </summary>
public class Cotizacion
{
    public int Id { get; set; }

    /// <summary>ID del usuario (vendedor) que generó la cotización.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    // ─── Relación con Cliente ─────────────────────────────────────────────────
    public int    ClienteId { get; set; }
    public Cliente Cliente  { get; set; } = null!;

    // ─── Resultado total ──────────────────────────────────────────────────────
    /// <summary>Suma de PrecioSubtotal de todos los detalles.</summary>
    public decimal PrecioTotal { get; set; }

    // ─── Detalles (mesones) ───────────────────────────────────────────────────
    public ICollection<DetalleCotizacion> Detalles { get; set; } = [];

    // ─── Metadata ─────────────────────────────────────────────────────────────
    public string?   Comentarios      { get; set; }
    public string    Estado           { get; set; } = "Cotizado";
    public DateTime  FechaCreacion    { get; set; } = DateTime.UtcNow;

    /// <summary>Se registra automáticamente cuando el estado pasa a "Aprobado".</summary>
    public DateTime? FechaAprobacion  { get; set; }
}
