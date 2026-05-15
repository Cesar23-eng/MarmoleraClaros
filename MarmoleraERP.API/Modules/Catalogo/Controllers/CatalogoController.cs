using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Catalogo.DTOs;
using MarmoleraERP.API.Modules.Catalogo.Entities;
using MarmoleraERP.API.Modules.Catalogo.Enums;

namespace MarmoleraERP.API.Modules.Catalogo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogoController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogoController(AppDbContext db) => _db = db;

    // ═══════════════════════════ MATERIALES ════════════════════════════════

    [HttpGet("materiales")]
    [Authorize(Roles = "Ventas,Produccion,Contabilidad,Admin")]
    public async Task<IActionResult> GetMateriales()
    {
        var materiales = await _db.Materiales
            .Select(m => new MaterialResponseDto(m.Id, m.Nombre, m.Categoria, m.PrecioPorM2, m.EstadoActivo))
            .ToListAsync();

        return Ok(materiales);
    }

    [HttpGet("materiales/{id:int}")]
    [Authorize(Roles = "Ventas,Produccion,Contabilidad,Admin")]
    public async Task<IActionResult> GetMaterial(int id)
    {
        var m = await _db.Materiales.FindAsync(id);
        if (m is null) return NotFound();
        return Ok(new MaterialResponseDto(m.Id, m.Nombre, m.Categoria, m.PrecioPorM2, m.EstadoActivo));
    }

    [HttpPost("materiales")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> CreateMaterial([FromBody] MaterialCreateDto dto)
    {
        var material = new Material
        {
            Nombre = dto.Nombre,
            Categoria = dto.Categoria,
            PrecioPorM2 = dto.PrecioPorM2
        };

        _db.Materiales.Add(material);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMaterial), new { id = material.Id },
            new MaterialResponseDto(material.Id, material.Nombre, material.Categoria,
                                    material.PrecioPorM2, material.EstadoActivo));
    }

    /// <summary>
    /// Modificar precio/datos de material. Restringido estrictamente a Contabilidad/Admin.
    /// </summary>
    [HttpPut("materiales/{id:int}")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> UpdateMaterial(int id, [FromBody] MaterialUpdateDto dto)
    {
        var material = await _db.Materiales.FindAsync(id);
        if (material is null) return NotFound();

        material.Nombre = dto.Nombre;
        material.Categoria = dto.Categoria;
        material.PrecioPorM2 = dto.PrecioPorM2;
        material.EstadoActivo = dto.EstadoActivo;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("materiales/{id:int}")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var material = await _db.Materiales.FindAsync(id);
        if (material is null) return NotFound();

        _db.Materiales.Remove(material);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ═══════════════════════════ SERVICIOS EXTRAS ══════════════════════════

    [HttpGet("servicios")]
    [Authorize(Roles = "Ventas,Produccion,Contabilidad,Admin")]
    public async Task<IActionResult> GetServicios()
    {
        var servicios = await _db.ServiciosExtras
            .Select(s => new ServicioExtraResponseDto(s.Id, s.Descripcion, s.TipoCobro.ToString(), s.Precio))
            .ToListAsync();

        return Ok(servicios);
    }

    [HttpGet("servicios/{id:int}")]
    [Authorize(Roles = "Ventas,Produccion,Contabilidad,Admin")]
    public async Task<IActionResult> GetServicio(int id)
    {
        var s = await _db.ServiciosExtras.FindAsync(id);
        if (s is null) return NotFound();
        return Ok(new ServicioExtraResponseDto(s.Id, s.Descripcion, s.TipoCobro.ToString(), s.Precio));
    }

    [HttpPost("servicios")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> CreateServicio([FromBody] ServicioExtraCreateDto dto)
    {
        if (!Enum.TryParse<TipoCobro>(dto.TipoCobro, ignoreCase: true, out var tipoCobro))
            return BadRequest(new { message = $"TipoCobro inválido. Valores: {string.Join(", ", Enum.GetNames<TipoCobro>())}" });

        var servicio = new ServicioExtra
        {
            Descripcion = dto.Descripcion,
            TipoCobro = tipoCobro,
            Precio = dto.Precio
        };

        _db.ServiciosExtras.Add(servicio);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServicio), new { id = servicio.Id },
            new ServicioExtraResponseDto(servicio.Id, servicio.Descripcion, servicio.TipoCobro.ToString(), servicio.Precio));
    }

    /// <summary>
    /// Modificar precio de servicio. Restringido estrictamente a Contabilidad/Admin.
    /// </summary>
    [HttpPut("servicios/{id:int}")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> UpdateServicio(int id, [FromBody] ServicioExtraUpdateDto dto)
    {
        var servicio = await _db.ServiciosExtras.FindAsync(id);
        if (servicio is null) return NotFound();

        if (!Enum.TryParse<TipoCobro>(dto.TipoCobro, ignoreCase: true, out var tipoCobro))
            return BadRequest(new { message = $"TipoCobro inválido. Valores: {string.Join(", ", Enum.GetNames<TipoCobro>())}" });

        servicio.Descripcion = dto.Descripcion;
        servicio.TipoCobro = tipoCobro;
        servicio.Precio = dto.Precio;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("servicios/{id:int}")]
    [Authorize(Roles = "Contabilidad,Admin")]
    public async Task<IActionResult> DeleteServicio(int id)
    {
        var servicio = await _db.ServiciosExtras.FindAsync(id);
        if (servicio is null) return NotFound();

        _db.ServiciosExtras.Remove(servicio);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
