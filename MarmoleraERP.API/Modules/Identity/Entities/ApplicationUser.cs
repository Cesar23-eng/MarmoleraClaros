using Microsoft.AspNetCore.Identity;

namespace MarmoleraERP.API.Modules.Identity.Entities;

/// <summary>
/// Usuario de la aplicación extendido de IdentityUser.
/// Los roles disponibles son: Admin, Ventas, Produccion, Contabilidad.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
