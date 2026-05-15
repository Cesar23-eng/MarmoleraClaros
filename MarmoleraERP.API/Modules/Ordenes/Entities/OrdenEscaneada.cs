namespace MarmoleraERP.API.Modules.Ordenes.Entities;

/// <summary>
/// Representa una orden física escaneada (foto) desde una tablet.
/// Se adjunta a un Pedido y actualiza su contador de órdenes.
/// Las tablets SOLO pueden subir órdenes — no ven precios ni clientes.
/// </summary>
public class OrdenEscaneada
{
    public int Id { get; set; }

    /// <summary>ID del pedido al que pertenece esta orden.</summary>
    public int PedidoId { get; set; }

    /// <summary>Ruta o URL del archivo de imagen almacenado.</summary>
    public string RutaArchivo { get; set; } = string.Empty;

    /// <summary>Nombre original del archivo subido.</summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>Tipo MIME del archivo (image/jpeg, image/png, application/pdf).</summary>
    public string TipoContenido { get; set; } = string.Empty;

    /// <summary>Tamaño del archivo en bytes.</summary>
    public long TamanoBytes { get; set; }

    /// <summary>ID del usuario (tablet) que subió la orden.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public int NumeroPrinted { get; set; } = 0;

    // Navegación
    public MarmoleraERP.API.Modules.Ventas.Entities.Pedido Pedido { get; set; } = null!;
}
