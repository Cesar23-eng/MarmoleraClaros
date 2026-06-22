using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Entities;

/// <summary>
/// Notificación dirigida a uno o varios roles del sistema.
/// </summary>
public class Notificacion
{
    public int    Id          { get; set; }

    /// <summary>Texto breve que aparece en el badge.</summary>
    public string Titulo      { get; set; } = string.Empty;

    /// <summary>Detalle completo del evento.</summary>
    public string Mensaje     { get; set; } = string.Empty;

    /// <summary>Rol destino (Admin, Ventas, Produccion, Contabilidad, Tablet).</summary>
    public string RolDestino  { get; set; } = string.Empty;

    public TipoNotificacion Tipo { get; set; } = TipoNotificacion.General;

    /// <summary>ID de la entidad relacionada (cotización, orden, etc.).</summary>
    public int?   ReferenciaId   { get; set; }

    public bool   Leida          { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
