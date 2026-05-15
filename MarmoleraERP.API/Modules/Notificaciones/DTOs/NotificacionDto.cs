namespace MarmoleraERP.API.Modules.Notificaciones.DTOs;

/// <summary>DTO de respuesta para una notificación.</summary>
public record NotificacionDto(
    int      Id,
    string   Tipo,
    int?     PedidoId,
    string   NumeroPedido,
    string   Mensaje,
    string   DestinoRol,
    bool     Leida,
    DateTime FechaCreacion,
    DateTime? FechaLectura
);

/// <summary>DTO para marcar notificaciones como leídas.</summary>
public record MarcarLeidaDto(List<int> Ids);
