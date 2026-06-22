namespace MarmoleraERP.API.Modules.Reportes.DTOs;

// ─── KPIs generales ──────────────────────────────────────────────────────
public record ResumenGeneralDto(
    int     TotalCotizaciones,
    int     CotizacionesAprobadas,
    int     CotizacionesEnProduccion,
    int     CotizacionesFinalizadas,
    decimal IngresosTotales,
    decimal IngresosEsteMes,
    int     TotalClientes,
    int     ClientesNuevosEsteMes
);

// ─── Ventas por mes ──────────────────────────────────────────────────────
public record VentasMesDto(
    int     Anio,
    int     Mes,
    string  MesNombre,
    decimal Total,
    int     Cantidad
);

// ─── Top materiales ──────────────────────────────────────────────────────
public record TopMaterialDto(
    string  NombreMaterial,
    int     VecesUsado,
    decimal AreaTotalM2,
    decimal IngresoGenerado
);

// ─── Distribución por estado ────────────────────────────────────────────────
public record EstadoDistribucionDto(
    string Estado,
    int    Cantidad,
    double Porcentaje
);

// ─── Top clientes ───────────────────────────────────────────────────────────
public record TopClienteDto(
    int     ClienteId,
    string  NombreCompleto,
    string  Telefono,
    int     TotalPedidos,
    decimal MontoTotal,
    string  UltimoPedidoEstado,
    DateTime UltimoPedidoFecha
);

// ─── Rendimiento de producción ───────────────────────────────────────────────
public record RendimientoProduccionDto(
    int     TotalOrdenes,
    int     OrdenesPorIniciar,
    int     OrdenesEnProduccion,
    int     OrdenesFinalizadas,
    double  TiempoPromedioHoras,      // Promedio desde FechaInicio hasta FechaFin
    double  TiempoMaximoHoras,
    double  TiempoMinimoHoras,
    int     OrdenesConOperario,       // Cuantas tienen operario asignado
    int     OrdenesSinOperario
);

// ─── Cotizaciones por rango de fechas ──────────────────────────────────────
public record CotizacionRangoItemDto(
    int      Id,
    string   NombreCliente,
    string   Estado,
    decimal  PrecioTotal,
    int      TotalPiezas,
    DateTime FechaCreacion,
    DateTime? FechaAprobacion
);

// ─── Resumen por vendedor (usuario con rol Ventas) ──────────────────────────
public record ResumenVendedorDto(
    string  UsuarioId,
    string  Nombre,
    string  Email,
    int     TotalCotizaciones,
    int     Aprobadas,
    int     Finalizadas,
    decimal MontoTotal,
    double  TasaConversion          // Aprobadas / TotalCotizaciones * 100
);

// ─── Ticket promedio por mes ─────────────────────────────────────────────────
public record TicketPromedioMesDto(
    int     Anio,
    int     Mes,
    string  MesNombre,
    decimal TicketPromedio,
    decimal MontoMaximo,
    decimal MontoMinimo,
    int     Cantidad
);
