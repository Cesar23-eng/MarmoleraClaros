using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Notificaciones.DTOs;
using MarmoleraERP.API.Modules.Notificaciones.Entities;

namespace MarmoleraERP.API.Modules.Notificaciones.Controllers;

/// <summary>
/// Gestiona las notificaciones dirigidas a roles del sistema.
/// Cada usuario ve solo las notificaciones de su propio rol.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController(AppDbContext db) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/notificaciones  — todas las del rol del usuario
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMias()
    {
        var rol = ObtenerRol();
        if (rol is null) return Unauthorized();

        var lista = await db.Notificaciones
            .Where(n => n.DestinoRol == rol || n.DestinoRol == "Admin")
            .OrderByDescending(n => n.FechaCreacion)
            .Take(100)
            .ToListAsync();

        return Ok(lista.Select(ToDto));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/notificaciones/no-leidas/count  — badge contador
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("no-leidas/count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountNoLeidas()
    {
        var rol = ObtenerRol();
        if (rol is null) return Unauthorized();

        var count = await db.Notificaciones
            .CountAsync(n => (n.DestinoRol == rol || n.DestinoRol == "Admin") && !n.Leida);

        return Ok(new { count });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/notificaciones  — crear (solo Admin)
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NotificacionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearNotificacionDto dto)
    {
        var notif = new Notificacion
        {
            Tipo          = dto.Tipo,
            Mensaje       = dto.Mensaje,
            DestinoRol    = dto.DestinoRol,
            ReferenciaId  = dto.ReferenciaId,
            FechaCreacion = DateTime.UtcNow,
        };

        db.Notificaciones.Add(notif);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMias), ToDto(notif));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/notificaciones/{id}/leer
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/leer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        var notif = await db.Notificaciones.FindAsync(id);
        if (notif is null) return NotFound();

        notif.Leida = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/notificaciones/leer-todas
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("leer-todas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var rol = ObtenerRol();
        if (rol is null) return Unauthorized();

        var pendientes = await db.Notificaciones
            .Where(n => (n.DestinoRol == rol || n.DestinoRol == "Admin") && !n.Leida)
            .ToListAsync();

        pendientes.ForEach(n => n.Leida = true);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DELETE /api/notificaciones/{id}  (solo Admin)
    // ════════════════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var notif = await db.Notificaciones.FindAsync(id);
        if (notif is null) return NotFound();

        db.Notificaciones.Remove(notif);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════════════
    private string? ObtenerRol() =>
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

    private static NotificacionDto ToDto(Notificacion n) => new(
        n.Id, n.Tipo.ToString(), n.Mensaje,
        n.DestinoRol, n.Leida, n.FechaCreacion, n.ReferenciaId
    );
}
