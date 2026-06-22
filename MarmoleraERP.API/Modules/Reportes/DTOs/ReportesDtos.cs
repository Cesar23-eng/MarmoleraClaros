namespace MarmoleraERP.API.Modules.Reportes.DTOs;

// ─── KPIs generales ───────────────────────────────────────────────────────────
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

// ─── Ventas por mes ───────────────────────────────────────────────────────────
public record VentasMesDto(
    int     Anio,
    int     Mes,
    string  MesNombre,
    decimal Total,
    int     Cantidad
);

// ─── Top materiales ───────────────────────────────────────────────────────────
public record TopMaterialDto(
    string  NombreMaterial,
    int     VecesUsado,
    decimal AreaTotalM2,
    decimal IngresoGenerado
);

// ─── Distribución por estado ────────────────────────────────────────────────────
public record EstadoDistribucionDto(
    string Estado,
    int    Cantidad,
    double Porcentaje
);
