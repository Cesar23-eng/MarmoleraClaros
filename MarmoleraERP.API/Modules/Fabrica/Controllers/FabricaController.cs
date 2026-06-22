using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Fabrica.DTOs;

namespace MarmoleraERP.API.Modules.Fabrica.Controllers;

/// <summary>
/// Tablero de fábrica: gestiona el flujo de producción de cotizaciones aprobadas.
/// Solo expone datos operativos (sin precios) al rol Produccion.
/// Flujo: Aprobado → EnProduccion → Finalizado
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Produccion,Admin")]
public class FabricaController(AppDbContext db) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/fabrica/por-iniciar  — Columna 1 del Kanban
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("por-iniciar")]
    [ProducesResponseType(typeof(List<CotizacionKanbanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorIniciar()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "Aprobado")
            .OrderBy(c => c.FechaAprobacion)
            .ToListAsync();

        return Ok(lista.Select(ToKanbanDto));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/fabrica/en-produccion  — Columna 2 del Kanban
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("en-produccion")]
    [ProducesResponseType(typeof(List<CotizacionKanbanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnProduccion()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "EnProduccion")
            .OrderBy(c => c.FechaAprobacion)
            .ToListAsync();

        return Ok(lista.Select(ToKanbanDto));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/fabrica/finalizados  — Columna 3 del Kanban
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("finalizados")]
    [ProducesResponseType(typeof(List<CotizacionKanbanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinalizados()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "Finalizado")
            .OrderByDescending(c => c.FechaAprobacion)
            .Take(50)
            .ToListAsync();

        return Ok(lista.Select(ToKanbanDto));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/fabrica/{id}/iniciar  — Aprobado → EnProduccion
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/iniciar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IniciarOrden(int id)
    {
        var cotizacion = await db.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización #{id} no encontrada." });

        if (cotizacion.Estado != "Aprobado")
            return BadRequest(new { mensaje = $"Solo se puede iniciar una orden en estado 'Aprobado'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "EnProduccion";
        await db.SaveChangesAsync();

        return Ok(new { mensaje = $"Orden #{id} iniciada en producción." });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/fabrica/{id}/finalizar  — EnProduccion → Finalizado
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizarOrden(int id)
    {
        var cotizacion = await db.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización #{id} no encontrada." });

        if (cotizacion.Estado != "EnProduccion")
            return BadRequest(new { mensaje = $"Solo se puede finalizar una orden en estado 'EnProduccion'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "Finalizado";
        await db.SaveChangesAsync();

        return Ok(new { mensaje = $"Orden #{id} finalizada y lista para entrega." });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════════════
    private static CotizacionKanbanDto ToKanbanDto(MarmoleraERP.API.Modules.Ventas.Entities.Cotizacion c) => new(
        Id:              c.Id,
        NombreCliente:   c.Cliente.NombreCompleto,
        Telefono:        c.Cliente.Telefono,
        FechaAprobacion: c.FechaAprobacion,
        Comentarios:     c.Comentarios,
        CantidadMesones: c.Detalles.Count,
        Mesones: c.Detalles.Select(d => new MesonKanbanDto(
            d.Id,
            d.NombreMaterial,
            d.Geometria.ToString(),
            d.AreaTotal
        )).ToList()
    );
}
