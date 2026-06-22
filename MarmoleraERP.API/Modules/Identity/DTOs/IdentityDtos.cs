namespace MarmoleraERP.API.Modules.Identity.DTOs;

// ── Auth ─────────────────────────────────────────────────────────────────────
public record LoginDto(
    string Email,
    string Password
);

public record LoginResponseDto(
    string Token,
    string UserId,
    string Email,
    string Nombre,
    List<string> Roles
);

public record RegisterDto(
    string Nombre,
    string Email,
    string Password,
    string Rol
);

// ── Gestión de usuarios (Admin) ─────────────────────────────────────────────────
public record UsuarioDto(
    string   Id,
    string   Nombre,
    string   Email,
    bool     Activo,
    DateTime FechaCreacion,
    List<string> Roles
);

public record CrearUsuarioDto(
    string Nombre,
    string Email,
    string Password,
    string Rol
);

public record EditarUsuarioDto(
    string  Nombre,
    string  Email,
    string  Rol,
    bool    Activo
);

public record CambiarPasswordDto(
    string NuevaPassword
);
