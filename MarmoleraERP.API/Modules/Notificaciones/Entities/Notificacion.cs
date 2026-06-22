using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Entities;

/// <summary>
/// Notificación dirigida a un rol específico del sistema.
/// Ejemplo: una cotización aprobada genera una notificación al rol Produccion.
/// </summary>
public class Notificacion
{
    public int              Id          { get; set; }
    public TipoNotificacion Tipo        { get; set; } = TipoNotificacion.General;
    public string           Mensaje     { get; set; } = string.Empty;

    /// <summary>Rol destino (Admin, Ventas, Produccion, Contabilidad, Tablet).</summary>
    public string  DestinoRol   { get; set; } = string.Empty;

    public bool    Leida        { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>ID opcional del recurso relacionado (cotización, pedido, evento).</summary>
    public int?    ReferenciaId  { get; set; }
}
