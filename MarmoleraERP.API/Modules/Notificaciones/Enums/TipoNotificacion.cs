namespace MarmoleraERP.API.Modules.Notificaciones.Enums;

/// <summary>
/// Tipos de notificaciones automáticas del sistema.
/// </summary>
public enum TipoNotificacion
{
    // Fábrica → Oficina
    PedidoTerminado,
    EntregaProgramada,
    ProblemaReportado,

    // Fábrica → Fábrica
    NuevaOrden,
    PedidoSinAvance,
    PedidoProximoEntrega,

    // Sistema → Contabilidad
    PedidoPendientePago
}
