namespace MarmoleraERP.API.Modules.Calendario.Enums;

/// <summary>
/// Tipo de evento en el calendario.
/// Determina el color visual en la UI:
///   TomaDeMedida    → Azul
///   EntregaEstimada → Amarillo
///   EntregaReal     → Verde
///   Problema        → Rojo
/// </summary>
public enum TipoEventoCalendario
{
    TomaDeMedida,
    EntregaEstimada,
    EntregaReal,
    Problema
}
