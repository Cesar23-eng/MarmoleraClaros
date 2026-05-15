using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Calendario.DTOs;
using MarmoleraERP.API.Modules.Calendario.Entities;
using MarmoleraERP.API.Modules.Calendario.Enums;
using MarmoleraERP.API.Modules.Notificaciones.Entities;
using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Calendario.Controllers;

/// <summary>
/// Calendario del sistema. Sin botón guardar — todo se persiste automáticamente.
/// Colores: Azul=TomaDeMedida, Amarillo=EntregaEstimada, Verde=EntregaReal, Rojo=Problema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarioController : ControllerBase
{
    private readonly AppDbContext _db;

    public CalendarioController(AppDbContext db) => _db = db;

    // ──────────────────────────────────────────────────────────────────────────
    // CONSULTAS
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve todos los eventos de un mes/año determinado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventoCalendarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventosMes([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio == 0) anio = DateTime.UtcNow.Year;
        if (mes  == 0) mes  = DateTime.UtcNow.Month;

        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin    = inicio.AddMonths(1);

        var eventos = await _db.EventosCalendario
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .OrderBy(e => e.Fecha)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(eventos);
    }

    /// <summary>
    /// Devuelve la carga diaria del mes (cantidad de eventos por día).
    /// Útil para colorear el calendario mensual.
    /// </summary>
    [HttpGet("carga-mensual")]
    [ProducesResponseType(typeof(List<CargaDiariaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCargaMensual([FromQuery] int anio, [FromQuery] int mes)
    {
        if (anio == 0) anio = DateTime.UtcNow.Year;
        if (mes  == 0) mes  = DateTime.UtcNow.Month;

        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin    = inicio.AddMonths(1);

        var carga = await _db.EventosCalendario
            .Where(e => e.Fecha >= inicio && e.Fecha < fin)
            .GroupBy(e => e.Fecha.Date)
            .Select(g => new CargaDiariaDto(DateOnly.FromDateTime(g.Key), g.Count()))
            .OrderBy(c => c.Dia)
            .ToListAsync();

        return Ok(carga);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CREAR EVENTO (auto-save)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un evento en el calendario. Se guarda automáticamente.
    /// Si el tipo es EntregaReal, notifica a oficina para contactar al cliente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Produccion,Ventas,Admin")]
    [ProducesResponseType(typeof(EventoCalendarioDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearEvento([FromBody] CrearEventoDto dto)
    {
        var evento = new EventoCalendario
        {
            PedidoId  = dto.PedidoId,
            Tipo      = dto.Tipo,
            Fecha     = dto.Fecha,
            Notas     = dto.Notas,
            UsuarioId = User.Identity?.Name ?? string.Empty
        };

        _db.EventosCalendario.Add(evento);

        // Si es entrega programada por fábrica → notificar a oficina
        if (dto.Tipo == TipoEventoCalendario.EntregaReal && dto.PedidoId.HasValue)
        {
            var notif = new Notificacion
            {
                Tipo          = TipoNotificacion.EntregaProgramada,
                PedidoId      = dto.PedidoId,
                Mensaje       = $"Pedido PED-{dto.PedidoId:D5} listo. Contactar al cliente para coordinar entrega el {dto.Fecha:dd/MM/yyyy}.",
                DestinoRol    = "Ventas",
                FechaCreacion = DateTime.UtcNow
            };
            _db.Notificaciones.Add(notif);
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEventosMes),
            new { anio = evento.Fecha.Year, mes = evento.Fecha.Month },
            MapToDto(evento));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // REPROGRAMAR / REPORTAR PROBLEMA
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reprograma un evento existente. Genera notificación a oficina.
    /// </summary>
    [HttpPut("{id:int}/reprogramar")]
    [Authorize(Roles = "Produccion,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reprogramar(int id, [FromBody] ReprogramarEventoDto dto)
    {
        var evento = await _db.EventosCalendario.FindAsync(id);
        if (evento is null) return NotFound(new { message = $"Evento {id} no encontrado." });

        evento.Fecha                 = dto.NuevaFecha;
        evento.Reprogramado          = true;
        evento.MotivoReprogramacion  = dto.Motivo;
        evento.FechaModificacion     = DateTime.UtcNow;

        var notif = new Notificacion
        {
            Tipo          = TipoNotificacion.ProblemaReportado,
            PedidoId      = evento.PedidoId,
            Mensaje       = $"Entrega reprogramada para {dto.NuevaFecha:dd/MM/yyyy}. Motivo: {dto.Motivo}",
            DestinoRol    = "Ventas",
            FechaCreacion = DateTime.UtcNow
        };
        _db.Notificaciones.Add(notif);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────────────────────────────────

    private static EventoCalendarioDto MapToDto(EventoCalendario e) =>
        new(
            e.Id,
            e.PedidoId,
            e.PedidoId.HasValue ? $"PED-{e.PedidoId:D5}" : "",
            e.Tipo.ToString(),
            ColorPorTipo(e.Tipo),
            e.Fecha,
            e.Notas,
            e.Reprogramado,
            e.MotivoReprogramacion,
            e.FechaCreacion
        );

    private static string ColorPorTipo(TipoEventoCalendario tipo) => tipo switch
    {
        TipoEventoCalendario.TomaDeMedida    => "#2196F3", // Azul
        TipoEventoCalendario.EntregaEstimada => "#FFC107", // Amarillo
        TipoEventoCalendario.EntregaReal     => "#4CAF50", // Verde
        TipoEventoCalendario.Problema        => "#F44336", // Rojo
        _                                    => "#9E9E9E"
    };
}
