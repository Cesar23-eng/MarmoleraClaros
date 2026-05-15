using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;

namespace MarmoleraERP.API.Modules.Fabrica.Controllers;

/// <summary>
/// Tablero de producción: gestiona las cotizaciones que ya ingresaron a fábrica.
/// Solo accesible por roles Produccion y Admin.
/// </summary>
[ApiController]
[Route("api/produccion")]
[Authorize(Roles = "Produccion,Admin")]
public class ProduccionController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProduccionController(AppDbContext context) => _context = context;

    // ─── Tablero de fábrica ───────────────────────────────────────────────────

    /// <summary>
    /// GET /api/produccion/pedidos
    /// Devuelve todas las cotizaciones cuyo estado NO sea "Cotizado"
    /// (es decir, que ya pasaron a fábrica).
    /// </summary>
    [HttpGet("pedidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPedidosProduccion()
    {
        var pedidos = await _context.Cotizaciones
            .Where(c => c.Estado != "Cotizado")
            .OrderBy(c => c.FechaCreacion)
            .ToListAsync();

        return Ok(pedidos);
    }

    // ─── Mover tarjeta (actualizar estado) ───────────────────────────────────

    /// <summary>
    /// PUT /api/produccion/pedidos/{id}/estado
    /// Actualiza el estado de una cotización en el tablero Kanban de producción.
    /// Estados válidos: "Pendiente", "EnCorte", "Pulido", "Terminado".
    /// </summary>
    [HttpPut("pedidos/{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoDto dto)
    {
        var cotizacion = await _context.Cotizaciones.FindAsync(id);

        if (cotizacion is null)
            return NotFound(new { message = $"Cotización con ID {id} no encontrada." });

        cotizacion.Estado = dto.NuevoEstado;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Estado actualizado.", id = cotizacion.Id, nuevoEstado = cotizacion.Estado });
    }
}

// ─── DTO inline (pequeño, no justifica fichero propio) ───────────────────────

/// <summary>
/// DTO mínimo para recibir el nuevo estado de una cotización.
/// </summary>
public record ActualizarEstadoDto(string NuevoEstado);
