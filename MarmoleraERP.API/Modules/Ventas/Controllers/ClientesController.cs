using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Ventas.DTOs;
using MarmoleraERP.API.Modules.Ventas.Entities;

namespace MarmoleraERP.API.Modules.Ventas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController(AppDbContext db) : ControllerBase
{
    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/clientes
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearCliente([FromBody] ClienteCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
            return BadRequest(new { mensaje = "El nombre del cliente es obligatorio." });

        var cliente = new Cliente
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Telefono       = dto.Telefono?.Trim() ?? string.Empty,
            Direccion      = dto.Direccion?.Trim() ?? string.Empty,
            Nit_Ci         = dto.Nit_Ci?.Trim()    ?? string.Empty,
            FechaRegistro  = DateTime.UtcNow
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var response = ToDto(cliente);
        return CreatedAtAction(nameof(BuscarClientes), new { q = cliente.NombreCompleto }, response);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/clientes/buscar?q=texto
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Busca clientes cuyo nombre contenga el texto indicado en el parámetro <c>q</c>.
    /// Devuelve hasta 20 resultados ordenados alfabéticamente.
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(List<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarClientes([FromQuery] string q = "")
    {
        var clientes = await db.Clientes
            .Where(c => c.NombreCompleto.Contains(q))
            .OrderBy(c => c.NombreCompleto)
            .Take(20)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(clientes);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/clientes  (lista completa paginada)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Devuelve todos los clientes registrados, ordenados por nombre.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos()
    {
        var clientes = await db.Clientes
            .OrderBy(c => c.NombreCompleto)
            .Select(c => ToDto(c))
            .ToListAsync();

        return Ok(clientes);
    }

    // ────────────────────────────────────────────────────────────────────────
    private static ClienteResponseDto ToDto(Cliente c) =>
        new(c.Id, c.NombreCompleto, c.Telefono, c.Direccion, c.Nit_Ci, c.FechaRegistro);
}
