using System.ComponentModel.DataAnnotations;
using MarmoleraERP.API.Modules.Calendario.Enums;

namespace MarmoleraERP.API.Modules.Calendario.DTOs;

// ─── INPUT DTOs ───────────────────────────────────────────────────────────────

public record CrearEventoDto(
    [Required, MaxLength(200)] string Titulo,
    TipoEvento Tipo,
    DateTime FechaInicio,
    DateTime FechaFin,
    string? Color,
    string? Notas,
    int? PedidoId
);

public record ActualizarEventoDto(
    [Required, MaxLength(200)] string Titulo,
    TipoEvento Tipo,
    DateTime FechaInicio,
    DateTime FechaFin,
    string? Color,
    string? Notas,
    int? PedidoId
);

public record ReprogramarEventoDto(
    DateTime NuevaFechaInicio,
    DateTime NuevaFechaFin,
    [Required, MaxLength(500)] string Motivo
);

// ─── OUTPUT DTO ───────────────────────────────────────────────────────────────

public record EventoCalendarioDto(
    int       Id,
    string    Titulo,
    string    Tipo,
    DateTime  FechaInicio,
    DateTime  FechaFin,
    string?   Color,
    string?   Notas,
    string    UsuarioId,
    int?      PedidoId,
    bool      FueReprogramado,
    string?   MotivoReprogramacion,
    DateTime? FechaOriginal,
    DateTime  FechaCreacion
);
