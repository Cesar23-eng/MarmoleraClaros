using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.DTOs;

public record NotificacionDto(
    int              Id,
    string           Titulo,
    string           Mensaje,
    string           RolDestino,
    TipoNotificacion Tipo,
    int?             ReferenciaId,
    bool             Leida,
    DateTime         FechaCreacion
);

public record CrearNotificacionDto(
    string           Titulo,
    string           Mensaje,
    string           RolDestino,
    TipoNotificacion Tipo,
    int?             ReferenciaId = null
);
