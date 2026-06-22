using MarmoleraERP.API.Data;
using MarmoleraERP.API.Modules.Notificaciones.Entities;
using MarmoleraERP.API.Modules.Notificaciones.Enums;

namespace MarmoleraERP.API.Modules.Notificaciones.Services;

/// <summary>
/// Implementación del servicio de notificaciones.
/// Usa AppDbContext directamente — no hay lógica de negocio compleja aquí,
/// solo persistencia de eventos ya ocurridos en otros módulos.
/// </summary>
public class NotificacionService(AppDbContext db) : INotificacionService
{
    public async Task NotificarAsync(
        string titulo, string mensaje, string rolDestino,
        TipoNotificacion tipo, int? referenciaId = null)
    {
        db.Notificaciones.Add(new Notificacion
        {
            Titulo       = titulo,
            Mensaje      = mensaje,
            RolDestino   = rolDestino,
            Tipo         = tipo,
            ReferenciaId = referenciaId,
            Leida        = false,
            FechaCreacion = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task NotificarMultiplesRolesAsync(
        string titulo, string mensaje, IEnumerable<string> roles,
        TipoNotificacion tipo, int? referenciaId = null)
    {
        foreach (var rol in roles)
        {
            db.Notificaciones.Add(new Notificacion
            {
                Titulo        = titulo,
                Mensaje       = mensaje,
                RolDestino    = rol,
                Tipo          = tipo,
                ReferenciaId  = referenciaId,
                Leida         = false,
                FechaCreacion = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
    }
}
