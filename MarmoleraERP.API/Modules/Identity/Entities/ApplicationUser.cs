using Microsoft.AspNetCore.Identity;

namespace MarmoleraERP.API.Modules.Identity.Entities;

/// <summary>
/// Usuario del sistema. Extiende IdentityUser con campos adicionales.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Nombre completo para mostrar en la UI.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Permite desactivar un usuario sin eliminarlo.</summary>
    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
