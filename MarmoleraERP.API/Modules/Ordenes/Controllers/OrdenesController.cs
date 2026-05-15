using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Ordenes.DTOs;
using MarmoleraERP.API.Modules.Ordenes.Entities;

namespace MarmoleraERP.API.Modules.Ordenes.Controllers;

/// <summary>
/// Módulo de órdenes escaneadas (tablets).
/// El rol "Tablet" SOLO puede subir órdenes — no accede a precios, clientes ni pedidos completos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tablet,Admin,Ventas,Produccion")]
public class OrdenesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public OrdenesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // VISTA TABLET: pedidos que aún no tienen ninguna orden subida
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// [Tablet] Devuelve los pedidos que todavía NO tienen órdenes subidas.
    /// Solo expone el ID y número de pedido — SIN precios ni datos de cliente.
    /// </summary>
    [HttpGet("pedidos-sin-orden")]
    [Authorize(Roles = "Tablet,Admin")]
    [ProducesResponseType(typeof(List<PedidoSinOrdenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPedidosSinOrden()
    {
        var idsConOrden = await _db.OrdenesEscaneadas
            .Select(o => o.PedidoId)
            .Distinct()
            .ToListAsync();

        var pedidos = await _db.Pedidos
            .Where(p => !idsConOrden.Contains(p.Id))
            .Select(p => new PedidoSinOrdenDto(p.Id, $"PED-{p.Id:D5}"))
            .ToListAsync();

        return Ok(pedidos);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SUBIR ORDEN (tablet)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// [Tablet] Sube una imagen de orden y la adjunta al pedido indicado.
    /// Actualiza automáticamente el contador de órdenes del pedido.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Tablet,Admin")]
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirOrden([FromBody] SubirOrdenDto dto)
    {
        var pedido = await _db.Pedidos.FindAsync(dto.PedidoId);
        if (pedido is null)
            return NotFound(new { message = $"Pedido {dto.PedidoId} no encontrado." });

        if (string.IsNullOrWhiteSpace(dto.ContenidoBase64))
            return BadRequest(new { message = "El contenido de la imagen es requerido." });

        // Guardar archivo en disco (carpeta wwwroot/ordenes)
        var carpeta   = Path.Combine(_env.WebRootPath ?? "wwwroot", "ordenes");
        Directory.CreateDirectory(carpeta);
        var extension = Path.GetExtension(dto.NombreArchivo).ToLowerInvariant();
        var nombreUnico = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreUnico);
        var bytes = Convert.FromBase64String(dto.ContenidoBase64);
        await System.IO.File.WriteAllBytesAsync(rutaCompleta, bytes);

        var orden = new OrdenEscaneada
        {
            PedidoId      = dto.PedidoId,
            NombreArchivo = dto.NombreArchivo,
            RutaArchivo   = $"/ordenes/{nombreUnico}",
            TipoContenido = dto.TipoContenido,
            TamanoBytes   = dto.TamanoBytes,
            UsuarioId     = User.Identity?.Name ?? "tablet"
        };

        _db.OrdenesEscaneadas.Add(orden);
        await _db.SaveChangesAsync();

        var response = new OrdenDto(
            orden.Id, orden.PedidoId, orden.NombreArchivo,
            orden.TipoContenido, orden.TamanoBytes,
            orden.NumeroPrinted, orden.FechaSubida
        );

        return CreatedAtAction(nameof(GetOrdenesPorPedido),
            new { pedidoId = orden.PedidoId }, response);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CONSULTAR ÓRDENES POR PEDIDO (oficina / fábrica)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve todas las órdenes adjuntas a un pedido.
    /// Visible para Ventas, Produccion y Admin.
    /// </summary>
    [HttpGet("pedido/{pedidoId:int}")]
    [Authorize(Roles = "Ventas,Produccion,Admin")]
    [ProducesResponseType(typeof(List<OrdenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdenesPorPedido(int pedidoId)
    {
        var ordenes = await _db.OrdenesEscaneadas
            .Where(o => o.PedidoId == pedidoId)
            .OrderBy(o => o.FechaSubida)
            .Select(o => new OrdenDto(
                o.Id, o.PedidoId, o.NombreArchivo,
                o.TipoContenido, o.TamanoBytes,
                o.NumeroPrinted, o.FechaSubida
            ))
            .ToListAsync();

        return Ok(ordenes);
    }

    /// <summary>
    /// Devuelve el conteo de órdenes por pedido (útil para la vista de oficina).
    /// </summary>
    [HttpGet("pedido/{pedidoId:int}/count")]
    [Authorize(Roles = "Ventas,Produccion,Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ContarOrdenes(int pedidoId)
    {
        var count = await _db.OrdenesEscaneadas
            .CountAsync(o => o.PedidoId == pedidoId);

        return Ok(new { pedidoId, cantidadOrdenes = count });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // REGISTRAR IMPRESIÓN
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Incrementa el contador de impresiones de una orden específica.
    /// </summary>
    [HttpPost("{id:int}/imprimir")]
    [Authorize(Roles = "Produccion,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarImpresion(int id)
    {
        var orden = await _db.OrdenesEscaneadas.FindAsync(id);
        if (orden is null) return NotFound(new { message = $"Orden {id} no encontrada." });

        orden.NumeroPrinted++;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
