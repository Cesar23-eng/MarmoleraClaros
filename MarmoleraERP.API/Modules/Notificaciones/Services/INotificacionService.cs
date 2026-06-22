using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Services;

/// <summary>
/// Contrato para crear notificaciones desde cualquier módulo.
/// Se inyecta en controllers mediante DI sin acoplamiento directo.
/// </summary>
public interface INotificacionService
{
    /// <summary>Crea una notificación para un único rol.</summary>
    Task NotificarAsync(string titulo, string mensaje, string rolDestino,
                        TipoNotificacion tipo, int? referenciaId = null);

    /// <summary>Crea la misma notificación para varios roles a la vez.</summary>
    Task NotificarMultiplesRolesAsync(string titulo, string mensaje, IEnumerable<string> roles,
                                      TipoNotificacion tipo, int? referenciaId = null);
}
