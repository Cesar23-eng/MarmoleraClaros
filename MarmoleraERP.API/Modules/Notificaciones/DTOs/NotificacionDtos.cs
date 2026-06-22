using System.ComponentModel.DataAnnotations;
using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.DTOs;

// ─── INPUT ───────────────────────────────────────────────────────────────

public record CrearNotificacionDto(
    TipoNotificacion Tipo,
    [Required, MaxLength(500)] string Mensaje,
    [Required, MaxLength(50)]  string DestinoRol,
    int? ReferenciaId = null
);

// ─── OUTPUT ───────────────────────────────────────────────────────────────

public record NotificacionDto(
    int              Id,
    string           Tipo,
    string           Mensaje,
    string           DestinoRol,
    bool             Leida,
    DateTime         FechaCreacion,
    int?             ReferenciaId
);
