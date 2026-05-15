using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Entities;

/// <summary>
/// Notificación interna del sistema.
/// Generadas automáticamente por fábrica → oficina y viceversa.
/// </summary>
public class Notificacion
{
    public int Id { get; set; }

    public TipoNotificacion Tipo { get; set; }

    /// <summary>ID del pedido relacionado (opcional).</summary>
    public int? PedidoId { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Rol de destino: "Ventas", "Produccion", "Admin", "Contabilidad".</summary>
    public string DestinoRol { get; set; } = string.Empty;

    public bool Leida { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaLectura { get; set; }
}
