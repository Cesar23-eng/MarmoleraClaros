using MarmoleraERP.API.Modules.Calendario.Enums;

namespace MarmoleraERP.API.Modules.Calendario.Entities;

/// <summary>
/// Evento en el calendario del sistema.
/// Colores:
///   Azul  = TomaDeMedida  (crea oficina)
///   Amarillo = EntregaEstimada (crea oficina)
///   Verde = EntregaReal / Programada (crea fábrica)
/// Todo se guarda automáticamente — sin botón "Guardar" explícito.
/// </summary>
public class EventoCalendario
{
    public int Id { get; set; }

    public int? PedidoId { get; set; }

    public TipoEventoCalendario Tipo { get; set; }

    public DateTime Fecha { get; set; }

    public string? Notas { get; set; }

    /// <summary>ID del usuario que creó el evento.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaModificacion { get; set; }

    /// <summary>Indica si el evento fue reprogramado.</summary>
    public bool Reprogramado { get; set; } = false;

    /// <summary>Motivo de reprogramación o problema reportado.</summary>
    public string? MotivoReprogramacion { get; set; }

    // Navegación
    public MarmoleraERP.API.Modules.Ventas.Entities.Pedido? Pedido { get; set; }
}
