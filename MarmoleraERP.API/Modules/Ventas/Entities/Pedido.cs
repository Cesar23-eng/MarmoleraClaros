using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Ventas.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntregaEstimada { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Cotizacion;
    public decimal TotalFinal { get; set; }

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public ICollection<DetallePedidoGeometria> Geometrias { get; set; } = [];
}
