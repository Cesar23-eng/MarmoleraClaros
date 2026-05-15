using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Ventas.DTOs;

// ─── Detalle DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Datos de un mesón individual enviados por el frontend al crear/editar.
/// El servidor calcula AreaTotal y PrecioSubtotal a partir de estos valores.
/// </summary>
public record DetalleCotizacionCreateDto(
    string             NombreMaterial,
    PlantillaGeometria Geometria,
    decimal            LadoA,
    decimal            LadoB,
    decimal?           LadoC,
    decimal?           Ancho,
    decimal            PrecioPorM2
);

/// <summary>Representación de un mesón tal como se devuelve al cliente.</summary>
public record DetalleCotizacionResponseDto(
    int     Id,
    string  NombreMaterial,
    string  Geometria,
    string  MedidasJson,
    decimal PrecioPorM2,
    decimal AreaTotal,
    decimal PrecioSubtotal
);

// ─── Cotización DTOs ──────────────────────────────────────────────────────────

/// <summary>
/// DTO de entrada para crear o editar una cotización completa (cabecera + detalles).
/// </summary>
public record CotizacionCreateDto(
    int                              ClienteId,
    string?                          Comentarios,
    List<DetalleCotizacionCreateDto> Detalles
);

/// <summary>
/// DTO de respuesta con la cabecera de la cotización y todos sus mesones.
/// </summary>
public record CotizacionResponseDto(
    int                                  Id,
    string?                              Comentarios,
    decimal                              PrecioTotal,
    string                               Estado,
    DateTime                             FechaCreacion,
    DateTime?                            FechaAprobacion,
    ClienteResponseDto                   Cliente,
    List<DetalleCotizacionResponseDto>   Detalles
);
