using MarmoleraERP.API.Modules.Catalogo.Entities;
using MarmoleraERP.API.Modules.Ventas.Enums;

namespace MarmoleraERP.API.Modules.Ventas.Entities;

public class DetallePedidoGeometria
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int MaterialId { get; set; }

    /// <summary>Forma usada como plantilla de cálculo de área.</summary>
    public PlantillaGeometria PlantillaUsada { get; set; }

    // Medidas en metros. El significado exacto depende de la PlantillaUsada.
    public decimal LadoA { get; set; }
    public decimal LadoB { get; set; }
    public decimal? LadoC { get; set; }  // Usado en Forma_U
    public decimal? Ancho { get; set; }  // Grosor/ancho de la pieza

    /// <summary>Área calculada en m² según la plantilla.</summary>
    public decimal AreaCalculadaM2 { get; set; }
    public decimal SubtotalMaterial { get; set; }

    // Navegación
    public Pedido Pedido { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public ICollection<DetallePedidoExtra> ServiciosExtras { get; set; } = [];
}
