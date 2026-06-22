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

namespace MarmoleraERP.API.Modules.Ventas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CotizacionesController(AppDbContext db) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/cotizaciones
    // ════════════════════════════════════════════════════════════════════════
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

        var errores   = new List<string>();
        var detalles  = new List<DetalleCotizacion>();

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

        return CreatedAtAction(nameof(ObtenerCotizacion),
            new { id = cotizacion.Id },
            ToDto(cotizacion, cliente));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
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

        var errores       = new List<string>();
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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/{id}
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/todas
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/aprobar
    //  ★ CREA OrdenFabrica automáticamente (si no existe ya)
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/aprobar")]
    [Authorize(Roles = "Ventas,Admin")]
    public async Task<IActionResult> AprobarCotizacion(int id)
    {
        var cotizacion = await db.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Cotizado")
            return BadRequest(new { mensaje = $"Solo se pueden aprobar cotizaciones en estado 'Cotizado'. Estado actual: '{cotizacion.Estado}'." });

        // 1. Cambiar estado de la cotización
        cotizacion.Estado          = "Aprobado";
        cotizacion.FechaAprobacion = DateTime.UtcNow;

        // 2. Crear OrdenFabrica si no existe ya (idempotente)
        var yaExiste = await db.OrdenesFabrica
            .AnyAsync(o => o.CotizacionId == id);

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

        return Ok(new { mensaje = $"Cotización {id} aprobada. Orden de fábrica creada y visible en el tablero de producción." });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/pendientes-produccion
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/iniciar-produccion
    //  ★ Sincroniza OrdenFabrica a EnProduccion
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/iniciar-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> IniciarProduccion(int id)
    {
        var cotizacion = await db.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "Aprobado")
            return BadRequest(new { mensaje = $"Solo se puede iniciar la producción de cotizaciones 'Aprobadas'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "EnProduccion";

        // Sincronizar OrdenFabrica
        var orden = await db.OrdenesFabrica
            .FirstOrDefaultAsync(o => o.CotizacionId == id);
        if (orden is not null)
        {
            orden.Estado      = EstadoOrden.EnProduccion;
            orden.FechaInicio = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new { mensaje = $"Cotización {id} pasó a 'En Producción'." });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/en-produccion
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/cotizaciones/terminados
    // ════════════════════════════════════════════════════════════════════════
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

    // ════════════════════════════════════════════════════════════════════════
    //  PUT /api/cotizaciones/{id}/finalizar-produccion
    //  ★ Sincroniza OrdenFabrica a Finalizado
    // ════════════════════════════════════════════════════════════════════════
    [HttpPut("{id:int}/finalizar-produccion")]
    [Authorize(Roles = "Produccion,Admin")]
    public async Task<IActionResult> FinalizarProduccion(int id)
    {
        var cotizacion = await db.Cotizaciones.FindAsync(id);
        if (cotizacion is null)
            return NotFound(new { mensaje = $"Cotización con ID {id} no encontrada." });

        if (cotizacion.Estado != "EnProduccion")
            return BadRequest(new { mensaje = $"Solo se puede finalizar la producción de cotizaciones 'EnProduccion'. Estado actual: '{cotizacion.Estado}'." });

        cotizacion.Estado = "Finalizado";

        // Sincronizar OrdenFabrica
        var orden = await db.OrdenesFabrica
            .FirstOrDefaultAsync(o => o.CotizacionId == id);
        if (orden is not null)
        {
            orden.Estado   = EstadoOrden.Finalizado;
            orden.FechaFin = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new { mensaje = $"Cotización {id} pasó a 'Finalizado'." });
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────────────
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
            _ => throw new ArgumentOutOfRangeException(nameof(d.Geometria), $"Plantilla no reconocida: {d.Geometria}")
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

    private static CotizacionResponseDto ToDto(Cotizacion c, Cliente cl) =>
        new(
            Id:              c.Id,
            Comentarios:     c.Comentarios,
            PrecioTotal:     c.PrecioTotal,
            Estado:          c.Estado,
            FechaCreacion:   c.FechaCreacion,
            FechaAprobacion: c.FechaAprobacion,
            Cliente: new ClienteResponseDto(cl.Id, cl.NombreCompleto, cl.Telefono,
                                            cl.Direccion, cl.Nit_Ci, cl.FechaRegistro),
            Detalles: c.Detalles.Select(d => new DetalleCotizacionResponseDto(
                d.Id, d.NombreMaterial, d.Geometria.ToString(),
                d.MedidasJson, d.PrecioPorM2, d.AreaTotal, d.PrecioSubtotal
            )).ToList()
        );
}
