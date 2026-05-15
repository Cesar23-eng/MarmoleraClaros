using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Ventas.DTOs;

// ─── Cliente DTOs ─────────────────────────────────────────────────────────────

public record ClienteCreateDto(
    string NombreCompleto,
    string Telefono,
    string Direccion,
    string? Nit_Ci
);

public record ClienteResponseDto(
    int Id,
    string NombreCompleto,
    string Telefono,
    string Direccion,
    string Nit_Ci,
    DateTime FechaRegistro
);

// ─── Pedido DTOs ──────────────────────────────────────────────────────────────

public record PedidoCreateDto(
    int ClienteId,
    DateTime? FechaEntregaEstimada,
    List<DetallePedidoGeometriaCreateDto> Geometrias
);

public record PedidoResponseDto(
    int Id,
    int ClienteId,
    string NombreCliente,
    DateTime FechaCreacion,
    DateTime? FechaEntregaEstimada,
    string Estado,
    decimal TotalFinal,
    List<DetallePedidoGeometriaResponseDto> Geometrias
);

// ─── Detalle Geometría DTOs ───────────────────────────────────────────────────

public record DetallePedidoGeometriaCreateDto(
    int MaterialId,
    PlantillaGeometria PlantillaUsada,
    decimal LadoA,
    decimal LadoB,
    decimal? LadoC,
    decimal? Ancho,
    List<DetallePedidoExtraCreateDto> ServiciosExtras
);

public record DetallePedidoGeometriaResponseDto(
    int Id,
    int MaterialId,
    string NombreMaterial,
    string PlantillaUsada,
    decimal LadoA,
    decimal LadoB,
    decimal? LadoC,
    decimal? Ancho,
    decimal AreaCalculadaM2,
    decimal SubtotalMaterial,
    List<DetallePedidoExtraResponseDto> ServiciosExtras
);

// ─── Detalle Extra DTOs ───────────────────────────────────────────────────────

public record DetallePedidoExtraCreateDto(
    int ServicioExtraId,
    decimal Cantidad
);

public record DetallePedidoExtraResponseDto(
    int Id,
    int ServicioExtraId,
    string DescripcionServicio,
    decimal Cantidad,
    decimal SubtotalExtra
);

// ─── Kanban DTO (SIN campos de precio) ───────────────────────────────────────
// Este DTO es exclusivo para el módulo de Fábrica (FabricaController).
// No expone ningún dato económico/financiero.

public record PedidoKanbanDto(
    int Id,
    int ClienteId,
    string NombreCliente,
    DateTime FechaCreacion,
    DateTime? FechaEntregaEstimada,
    string Estado,
    List<DetalleKanbanDto> Geometrias
);

public record DetalleKanbanDto(
    int Id,
    string NombreMaterial,
    string Categoria,
    string PlantillaUsada,
    decimal LadoA,
    decimal LadoB,
    decimal? LadoC,
    decimal? Ancho,
    decimal AreaCalculadaM2,
    List<string> ServiciosExtras  // Solo descripción, sin precios
);

public record CambiarEstadoDto(EstadoPedido NuevoEstado);
