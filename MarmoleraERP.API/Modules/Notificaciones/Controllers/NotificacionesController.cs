using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Notificaciones.DTOs;
using MarmoleraERP.API.Modules.Notificaciones.Entities;
using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Controllers;

/// <summary>
/// Gestión de notificaciones internas automáticas.
/// Cada rol solo ve sus propias notificaciones.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotificacionesController(AppDbContext db) => _db = db;

    // ──────────────────────────────────────────────────────────────────────────
    // CONSULTAR NOTIFICACIONES DEL ROL ACTIVO
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve las notificaciones pendientes para el rol del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisNotificaciones([FromQuery] bool soloNoLeidas = true)
    {
        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var query = _db.Notificaciones
            .Where(n => roles.Contains(n.DestinoRol));

        if (soloNoLeidas)
            query = query.Where(n => !n.Leida);

        var result = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto(
                n.Id,
                n.Tipo.ToString(),
                n.PedidoId,
                n.PedidoId.HasValue ? $"PED-{n.PedidoId:D5}" : "",
                n.Mensaje,
                n.DestinoRol,
                n.Leida,
                n.FechaCreacion,
                n.FechaLectura
            ))
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Devuelve el conteo de notificaciones no leídas (para el badge en la UI).
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var count = await _db.Notificaciones
            .CountAsync(n => roles.Contains(n.DestinoRol) && !n.Leida);

        return Ok(new { noLeidas = count });
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MARCAR COMO LEÍDAS
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Marca una o varias notificaciones como leídas.
    /// </summary>
    [HttpPut("marcar-leidas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarLeidas([FromBody] MarcarLeidaDto dto)
    {
        var notifs = await _db.Notificaciones
            .Where(n => dto.Ids.Contains(n.Id))
            .ToListAsync();

        foreach (var n in notifs)
        {
            n.Leida        = true;
            n.FechaLectura = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CREAR NOTIFICACIÓN MANUAL (solo Admin)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// [Admin] Crea una notificación manual para un rol específico.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearManual(
        [FromBody] (string Mensaje, string DestinoRol, int? PedidoId) body)
    {
        var notif = new Notificacion
        {
            Tipo          = TipoNotificacion.PedidoPendientePago,
            PedidoId      = body.PedidoId,
            Mensaje       = body.Mensaje,
            DestinoRol    = body.DestinoRol,
            FechaCreacion = DateTime.UtcNow
        };

        _db.Notificaciones.Add(notif);
        await _db.SaveChangesAsync();

        return Created();
    }
}
