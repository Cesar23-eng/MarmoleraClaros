using MarmoleraERP.API.Modules.Fabrica.Enums;

namespace MarmoleraERP.API.Modules.Fabrica.Entities;

/// <summary>
/// Registro de producción vinculado a una Cotizacion aprobada.
/// Una cotizacion aprobada genera exactamente una OrdenFabrica.
/// </summary>
public class OrdenFabrica
{
    public int    Id              { get; set; }

    // Referencia a la cotizacion (no FK directa para evitar ciclo, usamos el ID)
    public int    CotizacionId    { get; set; }

    public EstadoOrden Estado     { get; set; } = EstadoOrden.PorIniciar;

    /// <summary>UserId del operario asignado (nullable = sin asignar)</summary>
    public string? OperarioId     { get; set; }
    public string? OperarioNombre { get; set; }

    public string? Notas          { get; set; }

    public DateTime FechaCreacion  { get; set; } = DateTime.UtcNow;
    public DateTime? FechaInicio   { get; set; }
    public DateTime? FechaFin      { get; set; }
}
