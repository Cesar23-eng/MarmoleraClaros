namespace MarmoleraERP.API.Modules.Fabrica.DTOs;

// DTO exclusivo para el tablero Kanban de fábrica.
// Deliberadamente NO expone precios ni información financiera.

/// <summary>Resumen de una orden visible en el tablero de fábrica.</summary>
public record CotizacionKanbanDto(
    int       Id,
    string    NombreCliente,
    string    Telefono,
    DateTime? FechaAprobacion,
    string?   Comentarios,
    int       CantidadMesones,
    List<MesonKanbanDto> Mesones
);

/// <summary>Datos de un mesón visibles al operario (sin precio).</summary>
public record MesonKanbanDto(
    int     Id,
    string  NombreMaterial,
    string  Geometria,
    decimal AreaM2
);
