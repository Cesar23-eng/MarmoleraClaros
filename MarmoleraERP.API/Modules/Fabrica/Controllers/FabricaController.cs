using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Ventas.DTOs;
using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Fabrica.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Produccion,Admin")]
public class FabricaController : ControllerBase
{
    private readonly AppDbContext _db;

    public FabricaController(AppDbContext db) => _db = db;

    /// <summary>
    /// Tablero Kanban: devuelve pedidos en estados de producción activa.
    /// ⚠️ El DTO PedidoKanbanDto NO incluye ningún campo de precio o total.
    /// </summary>
    [HttpGet("kanban")]
    [ProducesResponseType(typeof(List<PedidoKanbanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKanban()
    {
        var estados = new[] { EstadoPedido.Aprobado, EstadoPedido.EnCorte, EstadoPedido.EnPulido, EstadoPedido.Listo };

        var pedidos = await _db.Pedidos
            .Where(p => estados.Contains(p.Estado))
            .Include(p => p.Cliente)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.Material)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.ServiciosExtras)
                    .ThenInclude(e => e.ServicioExtra)
            .Select(p => new PedidoKanbanDto(
                p.Id,
                p.ClienteId,
                p.Cliente.NombreCompleto,
                p.FechaCreacion,
                p.FechaEntregaEstimada,
                p.Estado.ToString(),
                p.Geometrias.Select(g => new DetalleKanbanDto(
                    g.Id,
                    g.Material.Nombre,
                    g.Material.Categoria,
                    g.PlantillaUsada.ToString(),
                    g.LadoA, g.LadoB, g.LadoC, g.Ancho,
                    g.AreaCalculadaM2,
                    g.ServiciosExtras.Select(e => e.ServicioExtra.Descripcion).ToList()
                )).ToList()
            ))
            .ToListAsync();

        return Ok(pedidos);
    }

    /// <summary>
    /// Actualiza el estado de un pedido en la línea de producción.
    /// </summary>
    [HttpPut("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido is null) return NotFound(new { message = $"Pedido {id} no encontrado." });

        pedido.Estado = dto.NuevoEstado;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
