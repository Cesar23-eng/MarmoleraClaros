using MarmoleraERP.API.Modules.Ventas.Entities;
using MarmoleraERP.API.Modules.Calendario.Enums;

namespace MarmoleraERP.API.Modules.Calendario.Entities;

/// <summary>
/// Evento en el calendario de la marmolera.
/// Puede estar vinculado a un Pedido o Cotización (opcionales).
/// </summary>
public class EventoCalendario
{
    public int        Id          { get; set; }
    public string     Titulo      { get; set; } = string.Empty;
    public TipoEvento Tipo        { get; set; } = TipoEvento.Otro;

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin    { get; set; }

    /// <summary>Color hex elegido por el usuario (ej. "#3B82F6").</summary>
    public string? Color { get; set; }
    public string? Notas { get; set; }

    /// <summary>Usuario que creó el evento.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    // ─── Relación opcional con Pedido ────────────────────────────────────
    public int?    PedidoId { get; set; }
    public Pedido? Pedido   { get; set; }

    // ─── Relación opcional con Cotización (visita de medición) ───────────
    public int?         CotizacionId { get; set; }
    public Cotizacion?  Cotizacion   { get; set; }

    /// <summary>Estado de la visita: Pendiente | Confirmada | Realizada | Cancelada</summary>
    public string EstadoVisita { get; set; } = "Pendiente";

    // ─── Reprogramación ───────────────────────────────────────────────────
    public bool      FueReprogramado      { get; set; } = false;
    public string?   MotivoReprogramacion { get; set; }
    public DateTime? FechaOriginal        { get; set; }

    // ─── Metadata ─────────────────────────────────────────────────────────
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
