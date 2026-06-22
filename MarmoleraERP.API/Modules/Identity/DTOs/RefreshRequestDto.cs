namespace MarmoleraERP.API.Modules.Identity.DTOs;

/// <summary>
/// Cuerpo del endpoint POST /api/auth/refresh.
/// </summary>
public record RefreshRequestDto(string RefreshToken);
