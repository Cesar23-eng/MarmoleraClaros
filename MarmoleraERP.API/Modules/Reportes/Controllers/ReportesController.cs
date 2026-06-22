using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Reportes.DTOs;

namespace MarmoleraERP.API.Modules.Reportes.Controllers;

/// <summary>
/// Métricas y reportes del ERP. Solo Admin y Contabilidad.
/// Todas las consultas son read-only sobre datos reales.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Contabilidad")]
public class ReportesController(AppDbContext db) : ControllerBase
{
    private static readonly string[] MESES =
        ["Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic"];

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/resumen
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenGeneralDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen()
    {
        var hoy       = DateTime.UtcNow;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var cotizaciones = await db.Cotizaciones.ToListAsync();
        var clientes     = await db.Clientes.ToListAsync();

        return Ok(new ResumenGeneralDto(
            TotalCotizaciones:          cotizaciones.Count,
            CotizacionesAprobadas:      cotizaciones.Count(c => c.Estado == "Aprobado"),
            CotizacionesEnProduccion:   cotizaciones.Count(c => c.Estado == "EnProduccion"),
            CotizacionesFinalizadas:    cotizaciones.Count(c => c.Estado == "Finalizado"),
            IngresosTotales:            cotizaciones.Where(c => c.Estado != "Cotizado").Sum(c => c.PrecioTotal),
            IngresosEsteMes:            cotizaciones.Where(c => c.Estado != "Cotizado" && c.FechaCreacion >= inicioMes).Sum(c => c.PrecioTotal),
            TotalClientes:              clientes.Count,
            ClientesNuevosEsteMes:      0  // Extender con FechaCreacion en Cliente si se agrega
        ));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/ventas-por-mes?meses=12
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("ventas-por-mes")]
    [ProducesResponseType(typeof(List<VentasMesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentasPorMes([FromQuery] int meses = 12)
    {
        var desde = DateTime.UtcNow.AddMonths(-meses + 1);
        var inicio = new DateTime(desde.Year, desde.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var cotizaciones = await db.Cotizaciones
            .Where(c => c.Estado != "Cotizado" && c.FechaCreacion >= inicio)
            .ToListAsync();

        var agrupado = cotizaciones
            .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
            .Select(g => new VentasMesDto(
                Anio:      g.Key.Year,
                Mes:       g.Key.Month,
                MesNombre: MESES[g.Key.Month - 1],
                Total:     g.Sum(c => c.PrecioTotal),
                Cantidad:  g.Count()
            ))
            .OrderBy(v => v.Anio).ThenBy(v => v.Mes)
            .ToList();

        // Rellenar meses sin datos con cero
        var resultado = new List<VentasMesDto>();
        for (int i = 0; i < meses; i++)
        {
            var fecha = inicio.AddMonths(i);
            var existente = agrupado.FirstOrDefault(v => v.Anio == fecha.Year && v.Mes == fecha.Month);
            resultado.Add(existente ?? new VentasMesDto(fecha.Year, fecha.Month, MESES[fecha.Month - 1], 0, 0));
        }

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/top-materiales?top=5
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("top-materiales")]
    [ProducesResponseType(typeof(List<TopMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopMateriales([FromQuery] int top = 5)
    {
        var detalles = await db.DetallesCotizacion
            .Include(d => d.Cotizacion)
            .Where(d => d.Cotizacion.Estado != "Cotizado")
            .ToListAsync();

        var resultado = detalles
            .GroupBy(d => d.NombreMaterial)
            .Select(g => new TopMaterialDto(
                NombreMaterial:  g.Key,
                VecesUsado:      g.Count(),
                AreaTotalM2:     g.Sum(d => d.AreaTotal),
                IngresoGenerado: g.Sum(d => d.PrecioSubtotal)
            ))
            .OrderByDescending(m => m.AreaTotalM2)
            .Take(top)
            .ToList();

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/cotizaciones-por-estado
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("cotizaciones-por-estado")]
    [ProducesResponseType(typeof(List<EstadoDistribucionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorEstado()
    {
        var cotizaciones = await db.Cotizaciones.ToListAsync();
        var total = cotizaciones.Count;
        if (total == 0) return Ok(new List<EstadoDistribucionDto>());

        var resultado = cotizaciones
            .GroupBy(c => c.Estado)
            .Select(g => new EstadoDistribucionDto(
                Estado:     g.Key,
                Cantidad:   g.Count(),
                Porcentaje: Math.Round((double)g.Count() / total * 100, 1)
            ))
            .OrderByDescending(e => e.Cantidad)
            .ToList();

        return Ok(resultado);
    }
}
