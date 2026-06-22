using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Calendario.DTOs;
using MarmoleraERP.API.Modules.Calendario.Entities;

namespace MarmoleraERP.API.Modules.Calendario.Controllers;

/// <summary>
/// CRUD de eventos del calendario + reprogramación.
/// Todos los roles autenticados pueden ver y crear eventos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarioController(AppDbContext db) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/calendario?anio=2026&mes=6
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet]
    [ProducesResponseType(typeof(List<EventoCalendarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorMes([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio == 0 || mes == 0)
        {
            var hoy = DateTime.UtcNow;
            anio = hoy.Year;
            mes  = hoy.Month;
        }

        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin    = inicio.AddMonths(1).AddTicks(-1);

        var eventos = await db.EventosCalendario
            .Where(e => e.FechaInicio <= fin && e.FechaFin >= inicio)
            .OrderBy(e => e.FechaInicio)
            .ToListAsync();

        return Ok(eventos.Select(ToDto));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/calendario/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var evento = await db.EventosCalendario.FindAsync(id);
        if (evento is null) return NotFound(new { mensaje = $"Evento #{id} no encontrado." });
        return Ok(ToDto(evento));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/calendario
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearEventoDto dto)
    {
        var evento = new EventoCalendario
        {
            Titulo       = dto.Titulo,
            Tipo         = dto.Tipo,
            FechaInicio  = dto.FechaInicio.ToUniversalTime(),
            FechaFin     = dto.FechaFin.ToUniversalTime(),
            Color        = dto.Color,
            Notas        = dto.Notas,
            PedidoId     = dto.PedidoId,
            UsuarioId    = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            FechaCreacion = DateTime.UtcNow,
        };

        db.EventosCalendario.Add(evento);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = evento.Id }, ToDto(evento));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/calendario/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEventoDto dto)
    {
        var evento = await db.EventosCalendario.FindAsync(id);
        if (evento is null) return NotFound(new { mensaje = $"Evento #{id} no encontrado." });

        evento.Titulo      = dto.Titulo;
        evento.Tipo        = dto.Tipo;
        evento.FechaInicio = dto.FechaInicio.ToUniversalTime();
        evento.FechaFin    = dto.FechaFin.ToUniversalTime();
        evento.Color       = dto.Color;
        evento.Notas       = dto.Notas;
        evento.PedidoId    = dto.PedidoId;

        await db.SaveChangesAsync();
        return Ok(ToDto(evento));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/calendario/{id}/reprogramar
    // ════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Reprograma un evento guardando la fecha original y el motivo.
    /// Registra automáticamente FueReprogramado = true.
    /// </summary>
    [HttpPut("{id:int}/reprogramar")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reprogramar(int id, [FromBody] ReprogramarEventoDto dto)
    {
        var evento = await db.EventosCalendario.FindAsync(id);
        if (evento is null) return NotFound(new { mensaje = $"Evento #{id} no encontrado." });

        // Guardar fecha original solo la primera vez
        if (!evento.FueReprogramado)
            evento.FechaOriginal = evento.FechaInicio;

        evento.FechaInicio          = dto.NuevaFechaInicio.ToUniversalTime();
        evento.FechaFin             = dto.NuevaFechaFin.ToUniversalTime();
        evento.FueReprogramado      = true;
        evento.MotivoReprogramacion = dto.Motivo;

        await db.SaveChangesAsync();
        return Ok(ToDto(evento));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DELETE /api/calendario/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var evento = await db.EventosCalendario.FindAsync(id);
        if (evento is null) return NotFound(new { mensaje = $"Evento #{id} no encontrado." });

        db.EventosCalendario.Remove(evento);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  HELPER
    // ════════════════════════════════════════════════════════════════════════
    private static EventoCalendarioDto ToDto(EventoCalendario e) => new(
        e.Id, e.Titulo, e.Tipo.ToString(),
        e.FechaInicio, e.FechaFin, e.Color, e.Notas,
        e.UsuarioId, e.PedidoId,
        e.FueReprogramado, e.MotivoReprogramacion, e.FechaOriginal,
        e.FechaCreacion
    );
}
