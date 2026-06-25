using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Ventas.DTOs;
using MarmoleraERP.API.Modules.Ventas.Entities;
using MarmoleraERP.API.Modules.Ventas.Enums;
using MarmoleraERP.API.Modules.Fabrica.Entities;
using MarmoleraERP.API.Modules.Fabrica.Enums;
using MarmoleraERP.API.Modules.Notificaciones;
using MarmoleraERP.API.Modules.Notificaciones.Services;
using MarmoleraERP.API.Modules.Calendario.Entities;
using MarmoleraERP.API.Modules.Calendario.Enums;

namespace MarmoleraERP.API.Modules.Ventas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CotizacionesController(
    AppDbContext db,
    INotificacionService notifSvc) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════════════
    //  POST /api/cotizaciones
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(typeof(CotizacionResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CrearCotizacion([FromBody] CotizacionCreateDto dto)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized(new { mensaje = "No se pudo identificar al usuario desde el token." });

        var cliente = await db.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
            return BadRequest(new { mensaje = $"No existe un cliente con ID {dto.ClienteId}." });

        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return BadRequest(new { mensaje = "La cotización debe incluir al menos un mesón." });

        var cotizacion = new Cotizacion
        {
            UsuarioId     = usuarioId,
            ClienteId     = dto.ClienteId,
            Comentarios   = dto.Comentarios,
            FechaCreacion = DateTime.UtcNow,
            Estado        = "Cotizado"
        };

        var errores  = new List<string>();
        var detalles = new List<DetalleCotizacion>();

        for (int i = 0; i < dto.Detalles.Count; i++)
        {
            var d = dto.Detalles[i];
            var (valido, errorMedida) = ValidarMedidas(d);
            if (!valido) { errores.Add($"Detalle {i + 1}: {errorMedida}"); continue; }

            var area = CalcularArea(d);
            if (area <= 0) { errores.Add($"Detalle {i + 1}: el área calculada debe ser mayor a 0."); continue; }

            detalles.Add(new DetalleCotizacion
            {
                NombreMaterial = d.NombreMaterial,
                Geometria      = d.Geometria,
                MedidasJson    = SerializarMedidas(d),
                PrecioPorM2    = d.PrecioPorM2,
                AreaTotal      = Math.Round(area, 4),
                PrecioSubtotal = Math.Round(area * d.PrecioPorM2, 2)
            });
        }

        if (errores.Count > 0)
            return BadRequest(new { mensaje = "Errores en los detalles.", errores });

        cotizacion.Detalles    = detalles;
        cotizacion.PrecioTotal = detalles.Sum(d => d.PrecioSubtotal);

        db.Cotizaciones.Add(cotizacion);
        await db.SaveChangesAsync();

        await notifSvc.CotizacionCreadaAsync(cotizacion.Id, cliente.NombreCompleto);

        return CreatedAtAction(nameof(ObtenerCotizacion),
            new { id = cotizacion.Id },
            ToDto(cotizacion, cliente));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}
    // ══════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Ventas,Admin")]
    [ProducesResponseType(typeof(CotizacionResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> EditarCotizacion(int id, [FromBody] CotizacionCreateDto dto)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Detalles)
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Cotizado")
            return BadRequest(new { mensaje = $"Solo se pueden editar cotizaciones en estado 'Cotizado'. Estado actual: '{cotizacion.Estado}'." });

        var cliente = await db.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
            return BadRequest(new { mensaje = $"No existe un cliente con ID {dto.ClienteId}." });

        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return BadRequest(new { mensaje = "La cotización debe incluir al menos un mesón." });

        var errores        = new List<string>();
        var nuevosDetalles = new List<DetalleCotizacion>();

        for (int i = 0; i < dto.Detalles.Count; i++)
        {
            var d = dto.Detalles[i];
            var (valido, errorMedida) = ValidarMedidas(d);
            if (!valido) { errores.Add($"Detalle {i + 1}: {errorMedida}"); continue; }

            var area = CalcularArea(d);
            if (area <= 0) { errores.Add($"Detalle {i + 1}: el área calculada debe ser mayor a 0."); continue; }

            nuevosDetalles.Add(new DetalleCotizacion
            {
                NombreMaterial = d.NombreMaterial,
                Geometria      = d.Geometria,
                MedidasJson    = SerializarMedidas(d),
                PrecioPorM2    = d.PrecioPorM2,
                AreaTotal      = Math.Round(area, 4),
                PrecioSubtotal = Math.Round(area * d.PrecioPorM2, 2)
            });
        }

        if (errores.Count > 0)
            return BadRequest(new { mensaje = "Errores en los detalles.", errores });

        db.DetallesCotizacion.RemoveRange(cotizacion.Detalles);
        cotizacion.ClienteId   = dto.ClienteId;
        cotizacion.Comentarios = dto.Comentarios;
        cotizacion.Detalles    = nuevosDetalles;
        cotizacion.PrecioTotal = nuevosDetalles.Sum(d => d.PrecioSubtotal);

        await db.SaveChangesAsync();
        return Ok(ToDto(cotizacion, cliente));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DELETE /api/cotizaciones/{id}  — solo estado "Cotizado"
    // ══════════════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Ventas,Admin")]
    public async Task<IActionResult> EliminarCotizacion(int id)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Cotizado")
            return BadRequest(new { mensaje = $"Solo se pueden eliminar cotizaciones en estado 'Cotizado'. Estado actual: '{cotizacion.Estado}'." });

        db.DetallesCotizacion.RemoveRange(cotizacion.Detalles);
        db.Cotizaciones.Remove(cotizacion);
        await db.SaveChangesAsync();

        return Ok(new { mensaje = $"Cotización #{id} eliminada correctamente." });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> ObtenerMisCotizaciones()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();

        return Ok(lista.Select(c => ToDto(c, c.Cliente)));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/{id}
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerCotizacion(int id)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        return Ok(ToDto(cotizacion, cotizacion.Cliente));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/todas
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("todas")]
    public async Task<IActionResult> ObtenerTodasLasCotizaciones()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();

        return Ok(lista.Select(c => ToDto(c, c.Cliente)));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/aprobar
    //  Body: { requiereMedicion: bool, fechaVisita?: string (ISO) }
    // ══════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/aprobar")]
    [Authorize(Roles = "Ventas,Admin")]
    public async Task<IActionResult> AprobarCotizacion(int id, [FromBody] AprobarCotizacionDto dto)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var cotizacion = await db.Cotizaciones
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Cotizado")
            return BadRequest(new { mensaje = $"Solo se pueden aprobar cotizaciones en estado 'Cotizado'. Estado actual: '{cotizacion.Estado}'." });

        // ── Estado según si requiere visita ──────────────────────────────────
        cotizacion.Estado          = dto.RequiereMedicion ? "PendienteVisita" : "Aprobado";
        cotizacion.FechaAprobacion = DateTime.UtcNow;

        // ── Crear visita de medición si aplica ───────────────────────────────
        if (dto.RequiereMedicion)
        {
            var fechaVisita = dto.FechaVisita ?? DateTime.UtcNow.AddDays(3);
            db.EventosCalendario.Add(new EventoCalendario
            {
                Titulo        = $"Visita medición — {cotizacion.Cliente!.NombreCompleto}",
                Tipo          = TipoEvento.Medicion,
                FechaInicio   = fechaVisita,
                FechaFin      = fechaVisita.AddHours(2),
                CotizacionId  = id,
                UsuarioId     = usuarioId,
                EstadoVisita  = "Pendiente",
                Color         = "#f59e0b",
                Notas         = dto.NotasVisita,
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            // Sin visita → orden de fábrica directamente
            var yaExiste = await db.OrdenesFabrica.AnyAsync(o => o.CotizacionId == id);
            if (!yaExiste)
            {
                db.OrdenesFabrica.Add(new OrdenFabrica
                {
                    CotizacionId  = id,
                    Estado        = EstadoOrden.PorIniciar,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        await notifSvc.CotizacionAprobadaAsync(id, cotizacion.Cliente!.NombreCompleto);

        var msg = dto.RequiereMedicion
            ? $"Cotización {id} aprobada. Visita de medición agendada."
            : $"Cotización {id} aprobada. Orden de fábrica creada.";

        return Ok(new { mensaje = msg });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/confirmar-visita
    //  La arquitecta confirma o reprograma la fecha de su visita
    // ══════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/confirmar-visita")]
    [Authorize(Roles = "Ventas,Admin")]
    public async Task<IActionResult> ConfirmarVisita(int id, [FromBody] ConfirmarVisitaDto dto)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización {id} no encontrada." });

        if (cotizacion.Estado != "PendienteVisita")
            return BadRequest(new { mensaje = $"La cotización no está en estado 'PendienteVisita'." });

        // Actualizar el evento de calendario
        var evento = await db.EventosCalendario
            .Where(e => e.CotizacionId == id && e.Tipo == TipoEvento.Medicion)
            .OrderByDescending(e => e.FechaCreacion)
            .FirstOrDefaultAsync();

        if (evento is not null)
        {
            if (evento.FechaInicio != dto.FechaConfirmada)
            {
                evento.FechaOriginal        = evento.FechaInicio;
                evento.FueReprogramado      = true;
                evento.MotivoReprogramacion = dto.Motivo;
            }
            evento.FechaInicio   = dto.FechaConfirmada;
            evento.FechaFin      = dto.FechaConfirmada.AddHours(2);
            evento.EstadoVisita  = "Confirmada";
        }

        // La cotización pasa a Aprobado + se crea la orden de fábrica
        cotizacion.Estado = "Aprobado";

        var yaExiste = await db.OrdenesFabrica.AnyAsync(o => o.CotizacionId == id);
        if (!yaExiste)
        {
            db.OrdenesFabrica.Add(new OrdenFabrica
            {
                CotizacionId  = id,
                Estado        = EstadoOrden.PorIniciar,
                FechaCreacion = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return Ok(new { mensaje = $"Visita confirmada para {dto.FechaConfirmada:dd/MM/yyyy}. Orden de fábrica creada." });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/visitas-pendientes
    //  Para el panel de la arquitecta
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("visitas-pendientes")]
    [Authorize(Roles = "Ventas,Admin")]
    public async Task<IActionResult> ObtenerVisitasPendientes()
    {
        var visitas = await db.EventosCalendario
            .Include(e => e.Cotizacion!)
                .ThenInclude(c => c!.Cliente)
            .Where(e => e.Tipo == TipoEvento.Medicion
                     && (e.EstadoVisita == "Pendiente" || e.EstadoVisita == "Confirmada"))
            .OrderBy(e => e.FechaInicio)
            .Select(e => new VisitaResponseDto(
                e.Id,
                e.CotizacionId!.Value,
                e.Cotizacion!.Cliente.NombreCompleto,
                e.Cotizacion!.Cliente.Telefono,
                e.Cotizacion!.Cliente.Direccion,
                e.FechaInicio,
                e.EstadoVisita,
                e.Notas,
                e.FueReprogramado,
                e.FechaOriginal
            ))
            .ToListAsync();

        return Ok(visitas);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/pendientes-produccion
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("pendientes-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> ObtenerPendientesProduccion()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "Aprobado")
            .OrderBy(c => c.FechaAprobacion)
            .ToListAsync();

        return Ok(lista.Select(c => ToDto(c, c.Cliente)));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/iniciar-produccion
    // ══════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/iniciar-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> IniciarProduccion(int id)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Aprobado")
            return BadRequest(new { mensaje = $"Solo se puede iniciar la producción de cotizaciones 'Aprobadas'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "EnProduccion";

        var orden = await db.OrdenesFabrica.FirstOrDefaultAsync(o => o.CotizacionId == id);
        if (orden is not null)
        {
            orden.Estado      = EstadoOrden.EnProduccion;
            orden.FechaInicio = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await notifSvc.OrdenIniciadaAsync(orden?.Id ?? id, cotizacion.Cliente!.NombreCompleto);

        return Ok(new { mensaje = $"Cotización {id} pasó a 'En Producción'." });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/en-produccion
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("en-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> ObtenerEnProduccion()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "EnProduccion")
            .OrderBy(c => c.FechaAprobacion)
            .ToListAsync();

        return Ok(lista.Select(c => ToDto(c, c.Cliente)));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/terminados
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("terminados")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> ObtenerTerminados()
    {
        var lista = await db.Cotizaciones
            .Include(c => c.Cliente)
            .Include(c => c.Detalles)
            .Where(c => c.Estado == "Finalizado")
            .OrderByDescending(c => c.FechaAprobacion)
            .ToListAsync();

        return Ok(lista.Select(c => ToDto(c, c.Cliente)));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/finalizar-produccion
    // ══════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/finalizar-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> FinalizarProduccion(int id)
    {
        var cotizacion = await db.Cotizaciones
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "EnProduccion")
            return BadRequest(new { mensaje = $"Solo se puede finalizar la producción de cotizaciones 'EnProduccion'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "Finalizado";

        var orden = await db.OrdenesFabrica.FirstOrDefaultAsync(o => o.CotizacionId == id);
        if (orden is not null)
        {
            orden.Estado   = EstadoOrden.Finalizado;
            orden.FechaFin = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await notifSvc.OrdenFinalizadaAsync(orden?.Id ?? id, cotizacion.Cliente!.NombreCompleto);

        return Ok(new { mensaje = $"Cotización {id} pasó a 'Finalizado'." });
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────
    private static decimal CalcularArea(DetalleCotizacionCreateDto d) =>
        d.Geometria switch
        {
            PlantillaGeometria.Rectangulo => d.LadoA * d.LadoB,
            PlantillaGeometria.Forma_L =>
                (d.LadoA * d.Ancho!.Value) + ((d.LadoB - d.Ancho.Value) * d.Ancho.Value),
            PlantillaGeometria.Forma_U =>
                (d.LadoA * d.Ancho!.Value)
                + ((d.LadoB - 2 * d.Ancho.Value) * d.Ancho.Value)
                + (d.LadoC!.Value * d.Ancho.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(d.Geometria))
        };

    private static (bool valido, string? error) ValidarMedidas(DetalleCotizacionCreateDto d) =>
        d.Geometria switch
        {
            PlantillaGeometria.Forma_L when d.Ancho is null =>
                (false, "El campo 'Ancho' es obligatorio para Forma L."),
            PlantillaGeometria.Forma_U when d.Ancho is null =>
                (false, "El campo 'Ancho' es obligatorio para Forma U."),
            PlantillaGeometria.Forma_U when d.LadoC is null =>
                (false, "El campo 'LadoC' es obligatorio para Forma U."),
            _ => (true, null)
        };

    private static string SerializarMedidas(DetalleCotizacionCreateDto d) =>
        JsonSerializer.Serialize(new { d.LadoA, d.LadoB, LadoC = d.LadoC, Ancho = d.Ancho });

    private static CotizacionResponseDto ToDto(Cotizacion c, Cliente? cliente) => new(
        c.Id,
        c.Comentarios,
        c.PrecioTotal,
        c.Estado,
        c.FechaCreacion,
        c.FechaAprobacion,
        new ClienteResponseDto(
            cliente!.Id,
            cliente.NombreCompleto,
            cliente.Telefono,
            cliente.Direccion,
            cliente.Nit_Ci,
            cliente.FechaRegistro
        ),
        c.Detalles.Select(d => new DetalleCotizacionResponseDto(
            d.Id,
            d.NombreMaterial,
            d.Geometria.ToString(),
            d.MedidasJson,
            d.PrecioPorM2,
            d.AreaTotal,
            d.PrecioSubtotal
        )).ToList()
    );
}
