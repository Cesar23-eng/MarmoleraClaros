namespace MarmoleraERP.API.Modules.Ordenes.DTOs;

/// <summary>DTO de respuesta para una orden escaneada (sin datos financieros).</summary>
public record OrdenDto(
    int    Id,
    int    PedidoId,
    string NombreArchivo,
    string TipoContenido,
    long   TamanoBytes,
    int    NumeroPrinted,
    DateTime FechaSubida
);

/// <summary>DTO para subir una nueva orden desde tablet. Solo requiere el PedidoId.</summary>
public record SubirOrdenDto(
    int    PedidoId,
    string NombreArchivo,
    string TipoContenido,
    long   TamanoBytes,
    /// <summary>Base64 del archivo de imagen para almacenamiento.</summary>
    string ContenidoBase64
);

/// <summary>Resumen de pedidos que todavía NO tienen ninguna orden subida (vista tablet).</summary>
public record PedidoSinOrdenDto(
    int    PedidoId,
    string NumeroPedido
);
