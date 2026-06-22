using MarmoleraERP.API.Modules.Notificaciones.Enums;
using MarmoleraERP.API.Modules.Notificaciones.Services;

namespace MarmoleraERP.API.Modules.Notificaciones;

/// <summary>
/// Métodos de extensión estáticos con los eventos de negocio que generan notificaciones.
/// Los controllers llaman a estos métodos después de sus operaciones principales.
/// Separar aquí la lógica evita contaminar los controllers con textos y roles.
/// </summary>
public static class NotificacionesDispatcher
{
    // ── Ventas ──────────────────────────────────────────────────────────────

    /// <summary>Al crear una cotización nueva → avisa al Admin.</summary>
    public static Task CotizacionCreadaAsync(
        this INotificacionService svc, int cotizacionId, string clienteNombre) =>
        svc.NotificarAsync(
            titulo:       "Nueva cotización",
            mensaje:      $"Se creó la cotización #{cotizacionId} para {clienteNombre}. Pendiente de revisión.",
            rolDestino:   "Admin",
            tipo:         TipoNotificacion.CotizacionCreada,
            referenciaId: cotizacionId);

    /// <summary>Al aprobar una cotización → avisa a Produccion y Tablet (fábrica).</summary>
    public static Task CotizacionAprobadaAsync(
        this INotificacionService svc, int cotizacionId, string clienteNombre) =>
        svc.NotificarMultiplesRolesAsync(
            titulo:       "Cotización aprobada — nueva orden",
            mensaje:      $"La cotización #{cotizacionId} de {clienteNombre} fue aprobada. Se generó una orden de fábrica.",
            roles:        ["Produccion", "Tablet"],
            tipo:         TipoNotificacion.CotizacionAprobada,
            referenciaId: cotizacionId);

    /// <summary>Al rechazar una cotización → avisa a Ventas.</summary>
    public static Task CotizacionRechazadaAsync(
        this INotificacionService svc, int cotizacionId, string clienteNombre) =>
        svc.NotificarAsync(
            titulo:       "Cotización rechazada",
            mensaje:      $"La cotización #{cotizacionId} de {clienteNombre} fue rechazada. Requiere revisión.",
            rolDestino:   "Ventas",
            tipo:         TipoNotificacion.CotizacionRechazada,
            referenciaId: cotizacionId);

    // ── Fábrica ─────────────────────────────────────────────────────────────

    /// <summary>Al iniciar producción de una orden → avisa a Admin y Ventas.</summary>
    public static Task OrdenIniciadaAsync(
        this INotificacionService svc, int ordenId, string clienteNombre) =>
        svc.NotificarMultiplesRolesAsync(
            titulo:       "Producción iniciada",
            mensaje:      $"La orden #{ordenId} ({clienteNombre}) entró en producción.",
            roles:        ["Admin", "Ventas"],
            tipo:         TipoNotificacion.OrdenFabricaIniciada,
            referenciaId: ordenId);

    /// <summary>Al finalizar una orden → avisa a Admin, Ventas y Contabilidad.</summary>
    public static Task OrdenFinalizadaAsync(
        this INotificacionService svc, int ordenId, string clienteNombre) =>
        svc.NotificarMultiplesRolesAsync(
            titulo:       "Orden finalizada — lista para entrega",
            mensaje:      $"La orden #{ordenId} ({clienteNombre}) finalizó producción y está lista para entregar.",
            roles:        ["Admin", "Ventas", "Contabilidad"],
            tipo:         TipoNotificacion.OrdenFabricaFinalizada,
            referenciaId: ordenId);
}
