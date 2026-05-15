using MarmoleraERP.API.Modules.Catalogo.Entities;

namespace MarmoleraERP.API.Modules.Ventas.Entities;

public class DetallePedidoExtra
{
    public int Id { get; set; }
    public int DetalleGeometriaId { get; set; }
    public int ServicioExtraId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal SubtotalExtra { get; set; }

    // Navegación
    public DetallePedidoGeometria DetalleGeometria { get; set; } = null!;
    public ServicioExtra ServicioExtra { get; set; } = null!;
}
