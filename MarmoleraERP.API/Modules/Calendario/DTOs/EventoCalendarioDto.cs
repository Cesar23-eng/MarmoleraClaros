using MarmoleraERP.API.Modules.Calendario.Enums;

namespace MarmoleraERP.API.Modules.Calendario.DTOs;

/// <summary>DTO de respuesta para un evento del calendario.</summary>
public record EventoCalendarioDto(
    int      Id,
    int?     PedidoId,
    string   NumeroPedido,
    string   Tipo,
    string   ColorHex,        // Derivado del Tipo para facilidad del frontend
    DateTime Fecha,
    string?  Notas,
    bool     Reprogramado,
    string?  MotivoReprogramacion,
    DateTime FechaCreacion
);

/// <summary>DTO para crear un evento. Se guarda automáticamente.</summary>
public record CrearEventoDto(
    int?     PedidoId,
    TipoEventoCalendario Tipo,
    DateTime Fecha,
    string?  Notas
);

/// <summary>DTO para reprogramar o reportar un problema en un evento.</summary>
public record ReprogramarEventoDto(
    DateTime NuevaFecha,
    string   Motivo
);

/// <summary>Resumen de carga diaria para el calendario mensual.</summary>
public record CargaDiariaDto(
    DateOnly Dia,
    int CantidadEventos
);
