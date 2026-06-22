using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Notificaciones.DTOs;

namespace MarmoleraERP.API.Modules.Notificaciones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController(AppDbContext db) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════════════════
    //  GET /api/notificaciones
    //  Devuelve las notificaciones del ROL del usuario autenticado.
    //  El frontend solo llama a este endpoint — no necesita saber su rol.
    // ══════════════════════════════════════════════════════════════════════════
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool soloNoLeidas = false)
    {
        // Extraer el primer rol del token JWT
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(rol))
            return Ok(new List<NotificacionDto>());

        var query = db.Notificaciones
            .Where(n => n.RolDestino == rol);

        if (soloNoLeidas)
            query = query.Where(n => !n.Leida);

        var notificaciones = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Take(50)   // límite razonable para el dropdown
            .Select(n => new NotificacionDto(
                n.Id, n.Titulo, n.Mensaje, n.RolDestino,
                n.Tipo, n.ReferenciaId, n.Leida, n.FechaCreacion))
            .ToListAsync();

        return Ok(notificaciones);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GET /api/notificaciones/conteo
    //  Badge del header: cuántas no leídas tiene el usuario actual.
    // ══════════════════════════════════════════════════════════════════════════
    [HttpGet("conteo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Conteo()
    {
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(rol))
            return Ok(new { total = 0 });

        var total = await db.Notificaciones
            .CountAsync(n => n.RolDestino == rol && !n.Leida);

        return Ok(new { total });
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PUT /api/notificaciones/{id}/leer
    // ══════════════════════════════════════════════════════════════════════════
    [HttpPut("{id}/leer")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        var notif = await db.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id && n.RolDestino == rol);

        if (notif is null) return NotFound();

        notif.Leida = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PUT /api/notificaciones/leer-todas
    // ══════════════════════════════════════════════════════════════════════════
    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(rol)) return NoContent();

        await db.Notificaciones
            .Where(n => n.RolDestino == rol && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));

        return NoContent();
    }
}
