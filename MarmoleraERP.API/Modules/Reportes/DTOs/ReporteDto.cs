namespace MarmoleraERP.API.Modules.Reportes.DTOs;

/// <summary>Resumen de proformas por día.</summary>
public record ProformasPorDiaDto(
    DateOnly Dia,
    int      Cantidad,
    decimal  TotalCotizado
);

/// <summary>Resumen de ventas en un período.</summary>
public record VentasPorPeriodoDto(
    int     Anio,
    int     Mes,
    int     CantidadPedidos,
    decimal TotalVendido,
    decimal TotalCobrado,
    decimal TotalPendiente
);

/// <summary>Ranking de mejores clientes.</summary>
public record MejorClienteDto(
    int     ClienteId,
    string  NombreCliente,
    int     CantidadPedidos,
    decimal TotalComprado
);

/// <summary>Materiales más vendidos.</summary>
public record MaterialMasVendidoDto(
    int     MaterialId,
    string  NombreMaterial,
    string  Categoria,
    decimal TotalM2Vendido,
    decimal TotalFacturado
);

/// <summary>Rendimiento de vendedor.</summary>
public record RendimientoVendedorDto(
    string  UsuarioId,
    string  NombreVendedor,
    int     CantidadCotizaciones,
    int     CotizacionesAprobadas,
    decimal TasaConversion,
    decimal TotalVendido
);

/// <summary>KPIs principales para el dashboard.</summary>
public record DashboardKpiDto(
    int     PedidosActivos,
    int     PedidosEntregadosMes,
    decimal VentasMes,
    decimal CobranzaMes,
    int     NotificacionesPendientes,
    int     OrdenesSubidasHoy
);
