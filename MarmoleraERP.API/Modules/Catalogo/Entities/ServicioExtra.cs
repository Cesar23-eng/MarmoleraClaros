using MarmoleraERP.API.Modules.Catalogo.Enums;

namespace MarmoleraERP.API.Modules.Catalogo.Entities;

public class ServicioExtra
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Define la unidad de cobro del servicio adicional.</summary>
    public TipoCobro TipoCobro { get; set; }
    public decimal Precio { get; set; }
}
