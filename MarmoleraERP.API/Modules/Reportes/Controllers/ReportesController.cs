using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Reportes.DTOs;
using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Reportes.Controllers;

/// <summary>
/// Reportes y métricas de negocio.
/// Accesible por Admin y Gerencia.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Contabilidad")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportesController(AppDbContext db) => _db = db;

    // ──────────────────────────────────────────────────────────────────────────
    // DASHBOARD KPIs
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// KPIs principales para el dashboard de gerencia.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardKpiDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var hoy   = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var estadosActivos = new[] { EstadoPedido.Aprobado, EstadoPedido.EnCorte, EstadoPedido.EnPulido, EstadoPedido.Listo };

        var pedidosActivos = await _db.Pedidos
            .CountAsync(p => estadosActivos.Contains(p.Estado));

        var pedidosEntregadosMes = await _db.Pedidos
            .CountAsync(p => p.Estado == EstadoPedido.Entregado && p.FechaCreacion >= inicioMes);

        var ventasMes = await _db.Pedidos
            .Where(p => p.FechaCreacion >= inicioMes)
            .SumAsync(p => (decimal?)p.TotalFinal) ?? 0;

        var notifPendientes = await _db.Notificaciones
            .CountAsync(n => !n.Leida);

        var ordenesHoy = await _db.OrdenesEscaneadas
            .CountAsync(o => o.FechaSubida.Date == hoy);

        return Ok(new DashboardKpiDto(
            pedidosActivos,
            pedidosEntregadosMes,
            ventasMes,
            0m, // CobranzaMes — por implementar con módulo de pagos
            notifPendientes,
            ordenesHoy
        ));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PROFORMAS POR DÍA
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Proformas generadas por día en un rango de fechas.
    /// </summary>
    [HttpGet("proformas-por-dia")]
    [ProducesResponseType(typeof(List<ProformasPorDiaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProformasPorDia(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var resultado = await _db.Cotizaciones
            .Where(c => c.FechaCreacion >= desde && c.FechaCreacion <= hasta)
            .GroupBy(c => c.FechaCreacion.Date)
            .Select(g => new ProformasPorDiaDto(
                DateOnly.FromDateTime(g.Key),
                g.Count(),
                g.Sum(c => c.PrecioTotal)
            ))
            .OrderBy(r => r.Dia)
            .ToListAsync();

        return Ok(resultado);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // VENTAS POR PERÍODO
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resumen de ventas agrupado por mes/año.
    /// </summary>
    [HttpGet("ventas-por-periodo")]
    [ProducesResponseType(typeof(List<VentasPorPeriodoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentasPorPeriodo(
        [FromQuery] int anioDesde = 0,
        [FromQuery] int anioHasta = 0)
    {
        if (anioDesde == 0) anioDesde = DateTime.UtcNow.Year;
        if (anioHasta == 0) anioHasta = DateTime.UtcNow.Year;

        var resultado = await _db.Pedidos
            .Where(p => p.FechaCreacion.Year >= anioDesde && p.FechaCreacion.Year <= anioHasta)
            .GroupBy(p => new { p.FechaCreacion.Year, p.FechaCreacion.Month })
            .Select(g => new VentasPorPeriodoDto(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(p => p.TotalFinal),
                0m, // TotalCobrado — por implementar con módulo de pagos
                g.Sum(p => p.TotalFinal) // TotalPendiente provisional
            ))
            .OrderBy(r => r.Anio).ThenBy(r => r.Mes)
            .ToListAsync();

        return Ok(resultado);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MEJORES CLIENTES
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ranking de mejores clientes por total comprado.
    /// </summary>
    [HttpGet("mejores-clientes")]
    [ProducesResponseType(typeof(List<MejorClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMejoresClientes([FromQuery] int top = 10)
    {
        var resultado = await _db.Pedidos
            .GroupBy(p => new { p.ClienteId, p.Cliente.NombreCompleto })
            .Select(g => new MejorClienteDto(
                g.Key.ClienteId,
                g.Key.NombreCompleto,
                g.Count(),
                g.Sum(p => p.TotalFinal)
            ))
            .OrderByDescending(r => r.TotalComprado)
            .Take(top)
            .ToListAsync();

        return Ok(resultado);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MATERIALES MÁS VENDIDOS
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Materiales más utilizados en pedidos.
    /// </summary>
    [HttpGet("materiales-mas-vendidos")]
    [ProducesResponseType(typeof(List<MaterialMasVendidoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterialesMasVendidos([FromQuery] int top = 10)
    {
        var resultado = await _db.DetallesGeometria
            .GroupBy(d => new { d.MaterialId, d.Material.Nombre, d.Material.Categoria })
            .Select(g => new MaterialMasVendidoDto(
                g.Key.MaterialId,
                g.Key.Nombre,
                g.Key.Categoria,
                g.Sum(d => d.AreaCalculadaM2),
                g.Sum(d => d.SubtotalMaterial)
            ))
            .OrderByDescending(r => r.TotalM2Vendido)
            .Take(top)
            .ToListAsync();

        return Ok(resultado);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RENDIMIENTO POR VENDEDOR
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Rendimiento por vendedor: cotizaciones, conversión y total vendido.
    /// </summary>
    [HttpGet("rendimiento-vendedores")]
    [ProducesResponseType(typeof(List<RendimientoVendedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRendimientoVendedores()
    {
        var resultado = await _db.Cotizaciones
            .GroupBy(c => c.UsuarioId)
            .Select(g => new
            {
                UsuarioId   = g.Key,
                Total       = g.Count(),
                Aprobadas   = g.Count(c => c.Estado == "Aprobado" || c.Estado == "EnProduccion" || c.Estado == "Finalizado"),
                TotalVendido = g.Where(c => c.Estado != "Cotizado")
                                .Sum(c => (decimal?)c.PrecioTotal) ?? 0
            })
            .ToListAsync();

        // Enriquecer con nombre del usuario
        var usuarios = await _db.Users
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        var dto = resultado.Select(r =>
        {
            var usr   = usuarios.FirstOrDefault(u => u.Id == r.UsuarioId);
            var tasa  = r.Total > 0 ? Math.Round((decimal)r.Aprobadas / r.Total * 100, 1) : 0;
            return new RendimientoVendedorDto(
                r.UsuarioId,
                usr?.UserName ?? r.UsuarioId,
                r.Total,
                r.Aprobadas,
                tasa,
                r.TotalVendido
            );
        }).OrderByDescending(r => r.TotalVendido).ToList();

        return Ok(dto);
    }
}
