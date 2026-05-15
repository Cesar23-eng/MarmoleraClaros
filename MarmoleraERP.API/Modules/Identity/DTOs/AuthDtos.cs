namespace MarmoleraERP.API.Modules.Identity.DTOs;

// ─── Request ─────────────────────────────────────────────────────────────────

public record LoginRequestDto(string Email, string Password);

public record RegisterRequestDto(string Email, string Password, string Role, string NombreCompleto);


// ─── Response ────────────────────────────────────────────────────────────────

public record AuthResponseDto(
    string Token,
    string Email,
    string NombreCompleto,
    string Rol,
    DateTime Expiracion
);
