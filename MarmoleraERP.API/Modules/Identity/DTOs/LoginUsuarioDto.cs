namespace MarmoleraERP.API.Modules.Identity.DTOs;

/// <summary>
/// DTO para login simplificado desde la app móvil/desktop.
/// El usuario escribe solo su nombre (ej: "juliana") sin el dominio.
/// El API construye automáticamente: juliana@marmolera.com
/// </summary>
public class LoginUsuarioDto
{
    /// <summary>Nombre de usuario sin dominio. Ej: "juliana", "cesar", "javier".</summary>
    public string Usuario  { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
