using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Reportes.DTOs;
using MarmoleraERP.API.Modules.Fabrica.Enums;

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
    //  KPIs principales del dashboard de Admin/Contabilidad
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
            TotalCotizaciones:        cotizaciones.Count,
            CotizacionesAprobadas:    cotizaciones.Count(c => c.Estado == "Aprobado"),
            CotizacionesEnProduccion: cotizaciones.Count(c => c.Estado == "EnProduccion"),
            CotizacionesFinalizadas:  cotizaciones.Count(c => c.Estado == "Finalizado"),
            IngresosTotales:          cotizaciones.Where(c => c.Estado == "Finalizado").Sum(c => c.PrecioTotal),
            IngresosEsteMes:          cotizaciones.Where(c => c.Estado == "Finalizado" && c.FechaCreacion >= inicioMes).Sum(c => c.PrecioTotal),
            TotalClientes:            clientes.Count,
            ClientesNuevosEsteMes:    clientes.Count(c => c.FechaRegistro >= inicioMes)
        ));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/ventas-por-mes?meses=12
    //  Serie temporal para gráfico de barras / línea en Flutter
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("ventas-por-mes")]
    [ProducesResponseType(typeof(List<VentasMesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentasPorMes([FromQuery] int meses = 12)
    {
        var desde  = DateTime.UtcNow.AddMonths(-meses + 1);
        var inicio = new DateTime(desde.Year, desde.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var cotizaciones = await db.Cotizaciones
            .Where(c => c.Estado == "Finalizado" && c.FechaCreacion >= inicio)
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
            var fecha     = inicio.AddMonths(i);
            var existente = agrupado.FirstOrDefault(v => v.Anio == fecha.Year && v.Mes == fecha.Month);
            resultado.Add(existente ?? new VentasMesDto(fecha.Year, fecha.Month, MESES[fecha.Month - 1], 0, 0));
        }

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/top-materiales?top=5
    //  Materiales más utilizados por área y por ingreso generado
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("top-materiales")]
    [ProducesResponseType(typeof(List<TopMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopMateriales([FromQuery] int top = 5)
    {
        var detalles = await db.DetallesCotizacion
            .Include(d => d.Cotizacion)
            .Where(d => d.Cotizacion.Estado == "Finalizado")
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
    //  Distribución porcentual para gráfico de torta
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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/top-clientes?top=10
    //  Clientes con mayor volumen de negocio (por monto finalizado)
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("top-clientes")]
    [ProducesResponseType(typeof(List<TopClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopClientes([FromQuery] int top = 10)
    {
        var cotizaciones = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Where(c => c.Estado == "Finalizado")
            .ToListAsync();

        var resultado = cotizaciones
            .GroupBy(c => c.ClienteId)
            .Select(g =>
            {
                var ultima = g.OrderByDescending(c => c.FechaCreacion).First();
                return new TopClienteDto(
                    ClienteId:          g.Key,
                    NombreCompleto:     ultima.Cliente!.NombreCompleto,
                    Telefono:           ultima.Cliente.Telefono,
                    TotalPedidos:       g.Count(),
                    MontoTotal:         g.Sum(c => c.PrecioTotal),
                    UltimoPedidoEstado: ultima.Estado,
                    UltimoPedidoFecha:  ultima.FechaCreacion
                );
            })
            .OrderByDescending(c => c.MontoTotal)
            .Take(top)
            .ToList();

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/rendimiento-produccion
    //  Métricas de tiempo real de fábrica (tiempos, operarios, backlog)
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("rendimiento-produccion")]
    [ProducesResponseType(typeof(RendimientoProduccionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRendimientoProduccion()
    {
        var ordenes = await db.OrdenesFabrica.ToListAsync();

        var finalizadas = ordenes
            .Where(o => o.Estado == EstadoOrden.Finalizado
                     && o.FechaInicio.HasValue
                     && o.FechaFin.HasValue)
            .ToList();

        var tiempos = finalizadas
            .Select(o => (o.FechaFin!.Value - o.FechaInicio!.Value).TotalHours)
            .ToList();

        return Ok(new RendimientoProduccionDto(
            TotalOrdenes:          ordenes.Count,
            OrdenesPorIniciar:     ordenes.Count(o => o.Estado == EstadoOrden.PorIniciar),
            OrdenesEnProduccion:   ordenes.Count(o => o.Estado == EstadoOrden.EnProduccion),
            OrdenesFinalizadas:    ordenes.Count(o => o.Estado == EstadoOrden.Finalizado),
            TiempoPromedioHoras:   tiempos.Count > 0 ? Math.Round(tiempos.Average(), 2) : 0,
            TiempoMaximoHoras:     tiempos.Count > 0 ? Math.Round(tiempos.Max(), 2)     : 0,
            TiempoMinimoHoras:     tiempos.Count > 0 ? Math.Round(tiempos.Min(), 2)     : 0,
            OrdenesConOperario:    ordenes.Count(o => !string.IsNullOrEmpty(o.OperarioId)),
            OrdenesSinOperario:    ordenes.Count(o => string.IsNullOrEmpty(o.OperarioId))
        ));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/cotizaciones-por-rango?desde=2026-01-01&hasta=2026-06-30
    //  Listado filtrado por fecha para exportar o revisar en pantalla
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("cotizaciones-por-rango")]
    [ProducesResponseType(typeof(List<CotizacionRangoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCotizacionesPorRango(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        if (desde > hasta)
            return BadRequest(new { mensaje = "'desde' no puede ser posterior a 'hasta'." });

        var hastaFin = hasta.Date.AddDays(1).ToUniversalTime(); // incluir el día completo
        var desdeUtc = DateTime.SpecifyKind(desde.Date, DateTimeKind.Utc);

        var cotizaciones = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.FechaCreacion >= desdeUtc && c.FechaCreacion < hastaFin)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();

        var resultado = cotizaciones.Select(c => new CotizacionRangoItemDto(
            Id:               c.Id,
            NombreCliente:    c.Cliente!.NombreCompleto,
            Estado:           c.Estado,
            PrecioTotal:      c.PrecioTotal,
            TotalPiezas:      c.Detalles.Count,
            FechaCreacion:    c.FechaCreacion,
            FechaAprobacion:  c.FechaAprobacion
        )).ToList();

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/resumen-por-vendedor
    //  Productividad de cada usuario con rol Ventas
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("resumen-por-vendedor")]
    [ProducesResponseType(typeof(List<ResumenVendedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumenPorVendedor()
    {
        // Obtener IDs de usuarios con rol Ventas
        var vendedorRoleId = await db.Roles
            .Where(r => r.Name == "Ventas")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (vendedorRoleId is null)
            return Ok(new List<ResumenVendedorDto>());

        var vendedorIds = await db.UserRoles
            .Where(ur => ur.RoleId == vendedorRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var usuarios = await db.Users
            .Where(u => vendedorIds.Contains(u.Id))
            .ToListAsync();

        var cotizaciones = await db.Cotizaciones
            .Where(c => vendedorIds.Contains(c.UsuarioId))
            .ToListAsync();

        var resultado = usuarios.Select(u =>
        {
            var cots  = cotizaciones.Where(c => c.UsuarioId == u.Id).ToList();
            var total = cots.Count;
            var aprobadas   = cots.Count(c => c.Estado != "Cotizado");
            var finalizadas = cots.Count(c => c.Estado == "Finalizado");

            return new ResumenVendedorDto(
                UsuarioId:        u.Id,
                Nombre:           u.Nombre,
                Email:            u.Email ?? "",
                TotalCotizaciones: total,
                Aprobadas:        aprobadas,
                Finalizadas:      finalizadas,
                MontoTotal:       cots.Where(c => c.Estado == "Finalizado").Sum(c => c.PrecioTotal),
                TasaConversion:   total > 0 ? Math.Round((double)aprobadas / total * 100, 1) : 0
            );
        })
        .OrderByDescending(v => v.MontoTotal)
        .ToList();

        return Ok(resultado);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/reportes/tickets-promedio-por-mes?meses=6
    //  Ticket promedio, máximo y mínimo por mes (para análisis de precios)
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("tickets-promedio-por-mes")]
    [ProducesResponseType(typeof(List<TicketPromedioMesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketsPromedio([FromQuery] int meses = 6)
    {
        var desde  = DateTime.UtcNow.AddMonths(-meses + 1);
        var inicio = new DateTime(desde.Year, desde.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var cotizaciones = await db.Cotizaciones
            .Where(c => c.Estado == "Finalizado" && c.FechaCreacion >= inicio)
            .ToListAsync();

        var agrupado = cotizaciones
            .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
            .Select(g => new TicketPromedioMesDto(
                Anio:           g.Key.Year,
                Mes:            g.Key.Month,
                MesNombre:      MESES[g.Key.Month - 1],
                TicketPromedio: Math.Round(g.Average(c => c.PrecioTotal), 2),
                MontoMaximo:    g.Max(c => c.PrecioTotal),
                MontoMinimo:    g.Min(c => c.PrecioTotal),
                Cantidad:       g.Count()
            ))
            .OrderBy(v => v.Anio).ThenBy(v => v.Mes)
            .ToList();

        // Rellenar meses sin datos
        var resultado = new List<TicketPromedioMesDto>();
        for (int i = 0; i < meses; i++)
        {
            var fecha     = inicio.AddMonths(i);
            var existente = agrupado.FirstOrDefault(v => v.Anio == fecha.Year && v.Mes == fecha.Month);
            resultado.Add(existente ?? new TicketPromedioMesDto(fecha.Year, fecha.Month, MESES[fecha.Month - 1], 0, 0, 0, 0));
        }

        return Ok(resultado);
    }
}
