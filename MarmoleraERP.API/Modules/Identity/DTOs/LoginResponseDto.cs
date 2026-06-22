namespace MarmoleraERP.API.Modules.Identity.DTOs;

/// <summary>
/// Respuesta del login: access token (JWT corto) + refresh token (opaco, largo).
/// </summary>
public record LoginResponseDto(
    string       AccessToken,
    string       RefreshToken,
    string       UserId,
    string       Email,
    string       Nombre,
    List<string> Roles
);
