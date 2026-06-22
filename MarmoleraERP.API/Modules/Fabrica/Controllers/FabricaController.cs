using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Fabrica.DTOs;
using MarmoleraERP.API.Modules.Fabrica.Entities;
using MarmoleraERP.API.Modules.Fabrica.Enums;

namespace MarmoleraERP.API.Modules.Fabrica.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Produccion,Tablet")]
public class FabricaController(AppDbContext db) : ControllerBase
{
    private async Task<List<CotizacionKanbanDto>> GetKanbanByEstado(EstadoOrden estado)
    {
        var ordenes = await db.OrdenesFabrica
            .Where(o => o.Estado == estado)
            .OrderBy(o => o.FechaCreacion)
            .ToListAsync();

        var ids = ordenes.Select(o => o.CotizacionId).ToList();

        var cotizaciones = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();

        return ordenes.Select(o =>
        {
            var cot = cotizaciones.FirstOrDefault(c => c.Id == o.CotizacionId);
            return new CotizacionKanbanDto(
                OrdenFabricaId: o.Id,
                CotizacionId:   o.CotizacionId,
                NombreCliente:  cot?.Cliente?.NombreCompleto ?? "Sin cliente",
                Telefono:       cot?.Cliente?.Telefono ?? "",
                Estado:         o.Estado.ToString(),
                OperarioNombre: o.OperarioNombre,
                Notas:          o.Notas,
                PrecioTotal:    cot?.PrecioTotal ?? 0,
                FechaCreacion:  o.FechaCreacion,
                FechaInicio:    o.FechaInicio,
                FechaFin:       o.FechaFin,
                TotalPiezas:    cot?.Detalles?.Count ?? 0
            );
        }).ToList();
    }

    [HttpGet("por-iniciar")]
    public async Task<IActionResult> GetPorIniciar() =>
        Ok(await GetKanbanByEstado(EstadoOrden.PorIniciar));

    [HttpGet("en-produccion")]
    public async Task<IActionResult> GetEnProduccion() =>
        Ok(await GetKanbanByEstado(EstadoOrden.EnProduccion));

    [HttpGet("finalizados")]
    public async Task<IActionResult> GetFinalizados() =>
        Ok(await GetKanbanByEstado(EstadoOrden.Finalizado));

    [HttpPost("{id:int}/iniciar")]
    public async Task<IActionResult> IniciarOrden(int id)
    {
        var orden = await db.OrdenesFabrica.FindAsync(id);
        if (orden is null) return NotFound();
        if (orden.Estado != EstadoOrden.PorIniciar)
            return BadRequest("La orden no está en estado PorIniciar.");

        orden.Estado      = EstadoOrden.EnProduccion;
        orden.FechaInicio = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/finalizar")]
    public async Task<IActionResult> FinalizarOrden(int id)
    {
        var orden = await db.OrdenesFabrica.FindAsync(id);
        if (orden is null) return NotFound();
        if (orden.Estado != EstadoOrden.EnProduccion)
            return BadRequest("La orden no está en producción.");

        orden.Estado   = EstadoOrden.Finalizado;
        orden.FechaFin = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/asignar-operario")]
    [Authorize(Roles = "Admin,Produccion")]
    public async Task<IActionResult> AsignarOperario(int id, [FromBody] AsignarOperarioDto dto)
    {
        var orden = await db.OrdenesFabrica.FindAsync(id);
        if (orden is null) return NotFound();

        orden.OperarioId     = dto.OperarioId;
        orden.OperarioNombre = dto.OperarioNombre;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/nota")]
    public async Task<IActionResult> AgregarNota(int id, [FromBody] AgregarNotaDto dto)
    {
        var orden = await db.OrdenesFabrica.FindAsync(id);
        if (orden is null) return NotFound();

        orden.Notas = dto.Nota;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
