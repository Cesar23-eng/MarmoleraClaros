namespace MarmoleraERP.API.Modules.Ventas.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    /// <summary>NIT para empresas / CI para personas naturales.</summary>
    public string Nit_Ci { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Pedido> Pedidos { get; set; } = [];
}
