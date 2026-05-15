namespace MarmoleraERP.API.Modules.Catalogo.Entities;

public class Material
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Precio por metro cuadrado en la moneda local.</summary>
    public decimal PrecioPorM2 { get; set; }
    public bool EstadoActivo { get; set; } = true;
}
