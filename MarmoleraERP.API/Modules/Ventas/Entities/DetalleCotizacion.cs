using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Ventas.Entities;

/// <summary>
/// Representa un mesón individual dentro de una cotización.
/// Una cotización (cabecera) puede tener N detalles.
/// </summary>
public class DetalleCotizacion
{
    public int Id { get; set; }

    // ─── Relación con la cabecera ─────────────────────────────────────────────
    public int CotizacionId { get; set; }
    public Cotizacion Cotizacion { get; set; } = null!;

    // ─── Material ─────────────────────────────────────────────────────────────
    /// <summary>Nombre del material elegido (texto libre, igual al catálogo).</summary>
    public string NombreMaterial { get; set; } = string.Empty;

    // ─── Geometría ────────────────────────────────────────────────────────────
    public PlantillaGeometria Geometria { get; set; }

    /// <summary>
    /// JSON serializado con las medidas: { "LadoA": 2.0, "LadoB": 1.5, "LadoC": null, "Ancho": null }.
    /// Se guarda como texto para no multiplicar columnas opcionales en la BD.
    /// </summary>
    public string MedidasJson { get; set; } = "{}";

    // ─── Resultados calculados ────────────────────────────────────────────────
    public decimal PrecioPorM2    { get; set; }
    public decimal AreaTotal      { get; set; }
    public decimal PrecioSubtotal { get; set; }
}
