namespace MarmoleraERP.API.Modules.Fabrica.DTOs;

// ── Tarjeta Kanban (lo que ve el tablero) ─────────────────────────────────────
public record CotizacionKanbanDto(
    int     OrdenFabricaId,
    int     CotizacionId,
    string  NombreCliente,
    string  Telefono,
    string  Estado,
    string? OperarioNombre,
    string? Notas,
    decimal PrecioTotal,
    DateTime FechaCreacion,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    int     TotalPiezas
);

// ── Asignar operario ─────────────────────────────────────────────────────────
public record AsignarOperarioDto(
    string OperarioId,
    string OperarioNombre
);

// ── Agregar nota ─────────────────────────────────────────────────────────────
public record AgregarNotaDto(string Nota);
