using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Ventas.DTOs;
using MarmoleraERP.API.Modules.Ventas.Entities;

namespace MarmoleraERP.API.Modules.Ventas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Ventas,Admin")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;

    public VentasController(AppDbContext db) => _db = db;

    // ═══════════════════════════ CLIENTES ══════════════════════════════════

    [HttpGet("clientes")]
    public async Task<IActionResult> GetClientes()
    {
        var clientes = await _db.Clientes
            .Select(c => new ClienteResponseDto(c.Id, c.NombreCompleto, c.Telefono,
                                                c.Direccion, c.Nit_Ci, c.FechaRegistro))
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("clientes/{id:int}")]
    public async Task<IActionResult> GetCliente(int id)
    {
        var c = await _db.Clientes.FindAsync(id);
        if (c is null) return NotFound();

        return Ok(new ClienteResponseDto(c.Id, c.NombreCompleto, c.Telefono,
                                         c.Direccion, c.Nit_Ci, c.FechaRegistro));
    }

    [HttpPost("clientes")]
    public async Task<IActionResult> CreateCliente([FromBody] ClienteCreateDto dto)
    {
        var cliente = new Cliente
        {
            NombreCompleto = dto.NombreCompleto,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Nit_Ci = dto.Nit_Ci ?? string.Empty
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        var response = new ClienteResponseDto(cliente.Id, cliente.NombreCompleto,
            cliente.Telefono, cliente.Direccion, cliente.Nit_Ci, cliente.FechaRegistro);

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, response);
    }

    [HttpPut("clientes/{id:int}")]
    public async Task<IActionResult> UpdateCliente(int id, [FromBody] ClienteCreateDto dto)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        cliente.NombreCompleto = dto.NombreCompleto;
        cliente.Telefono = dto.Telefono;
        cliente.Direccion = dto.Direccion;
        cliente.Nit_Ci = dto.Nit_Ci ?? string.Empty;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("clientes/{id:int}")]
    public async Task<IActionResult> DeleteCliente(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ═══════════════════════════ PEDIDOS ═══════════════════════════════════

    [HttpGet("pedidos")]
    public async Task<IActionResult> GetPedidos()
    {
        // TODO: Implementar lógica de proyección completa con Geometrias
        var pedidos = await _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.Material)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.ServiciosExtras)
                    .ThenInclude(e => e.ServicioExtra)
            .ToListAsync();

        return Ok(pedidos.Select(MapToPedidoResponseDto));
    }

    [HttpGet("pedidos/{id:int}")]
    public async Task<IActionResult> GetPedido(int id)
    {
        var p = await _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.Material)
            .Include(p => p.Geometrias)
                .ThenInclude(g => g.ServiciosExtras)
                    .ThenInclude(e => e.ServicioExtra)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p is null) return NotFound();
        return Ok(MapToPedidoResponseDto(p));
    }

    /// <summary>
    /// Crea una cotización/pedido nuevo.
    /// El área y subtotal de cada pieza se calculan del lado del servidor.
    /// </summary>
    [HttpPost("pedidos")]
    public async Task<IActionResult> CreatePedido([FromBody] PedidoCreateDto dto)
    {
        var cliente = await _db.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null) return BadRequest(new { message = "ClienteId no existe." });

        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            FechaEntregaEstimada = dto.FechaEntregaEstimada,
            Geometrias = []
        };

        decimal totalFinal = 0;

        foreach (var geoDto in dto.Geometrias)
        {
            var material = await _db.Materiales.FindAsync(geoDto.MaterialId);
            if (material is null) return BadRequest(new { message = $"MaterialId {geoDto.MaterialId} no existe." });

            var area = CalcularArea(geoDto);
            var subtotalMaterial = area * material.PrecioPorM2;

            var geometria = new DetallePedidoGeometria
            {
                MaterialId = geoDto.MaterialId,
                PlantillaUsada = geoDto.PlantillaUsada,
                LadoA = geoDto.LadoA,
                LadoB = geoDto.LadoB,
                LadoC = geoDto.LadoC,
                Ancho = geoDto.Ancho,
                AreaCalculadaM2 = area,
                SubtotalMaterial = subtotalMaterial,
                ServiciosExtras = []
            };

            decimal subtotalExtras = 0;
            foreach (var extraDto in geoDto.ServiciosExtras)
            {
                var servicio = await _db.ServiciosExtras.FindAsync(extraDto.ServicioExtraId);
                if (servicio is null) return BadRequest(new { message = $"ServicioExtraId {extraDto.ServicioExtraId} no existe." });

                var subtotalExtra = servicio.Precio * extraDto.Cantidad;
                subtotalExtras += subtotalExtra;

                geometria.ServiciosExtras.Add(new DetallePedidoExtra
                {
                    ServicioExtraId = extraDto.ServicioExtraId,
                    Cantidad = extraDto.Cantidad,
                    SubtotalExtra = subtotalExtra
                });
            }

            totalFinal += subtotalMaterial + subtotalExtras;
            pedido.Geometrias.Add(geometria);
        }

        pedido.TotalFinal = totalFinal;

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id },
            new { pedido.Id, pedido.TotalFinal, pedido.Estado });
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    private static decimal CalcularArea(DetallePedidoGeometriaCreateDto dto) =>
        dto.PlantillaUsada switch
        {
            // Rectángulo simple: A × B
            Enums.PlantillaGeometria.Rectangulo =>
                dto.LadoA * dto.LadoB,

            // Forma L (1 esquina compartida):
            //   Brazo 1 completo: LadoA × Ancho
            //   Brazo 2 sin la esquina: (LadoB - Ancho) × Ancho
            //   Ejemplo: L con A=2m, B=2m, Ancho=0.60m
            //   → (2 × 0.60) + ((2 - 0.60) × 0.60) = 1.20 + 0.84 = 2.04 m²
            Enums.PlantillaGeometria.Forma_L =>
                (dto.LadoA * (dto.Ancho ?? 0))
                + ((dto.LadoB - (dto.Ancho ?? 0)) * (dto.Ancho ?? 0)),

            // Forma U (2 esquinas compartidas):
            //   Brazo izquierdo completo:   LadoA × Ancho
            //   Brazo derecho completo:     LadoC × Ancho
            //   Barra central sin 2 esquinas: (LadoB - 2×Ancho) × Ancho
            Enums.PlantillaGeometria.Forma_U =>
                (dto.LadoA * (dto.Ancho ?? 0))
                + ((dto.LadoB - 2 * (dto.Ancho ?? 0)) * (dto.Ancho ?? 0))
                + ((dto.LadoC ?? 0) * (dto.Ancho ?? 0)),

            _ => 0
        };

    private static PedidoResponseDto MapToPedidoResponseDto(Pedido p) =>
        new(
            p.Id,
            p.ClienteId,
            p.Cliente.NombreCompleto,
            p.FechaCreacion,
            p.FechaEntregaEstimada,
            p.Estado.ToString(),
            p.TotalFinal,
            p.Geometrias.Select(g => new DetallePedidoGeometriaResponseDto(
                g.Id,
                g.MaterialId,
                g.Material.Nombre,
                g.PlantillaUsada.ToString(),
                g.LadoA, g.LadoB, g.LadoC, g.Ancho,
                g.AreaCalculadaM2,
                g.SubtotalMaterial,
                g.ServiciosExtras.Select(e => new DetallePedidoExtraResponseDto(
                    e.Id,
                    e.ServicioExtraId,
                    e.ServicioExtra.Descripcion,
                    e.Cantidad,
                    e.SubtotalExtra
                )).ToList()
            )).ToList()
        );
}
