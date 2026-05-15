namespace MarmoleraERP.API.Modules.Catalogo.DTOs;

// ─── Material DTOs ────────────────────────────────────────────────────────────

public record MaterialCreateDto(
    string Nombre,
    string Categoria,
    decimal PrecioPorM2
);

public record MaterialUpdateDto(
    string Nombre,
    string Categoria,
    decimal PrecioPorM2,
    bool EstadoActivo
);

public record MaterialResponseDto(
    int Id,
    string Nombre,
    string Categoria,
    decimal PrecioPorM2,
    bool EstadoActivo
);

// ─── ServicioExtra DTOs ───────────────────────────────────────────────────────

public record ServicioExtraCreateDto(
    string Descripcion,
    string TipoCobro,
    decimal Precio
);

public record ServicioExtraUpdateDto(
    string Descripcion,
    string TipoCobro,
    decimal Precio
);

public record ServicioExtraResponseDto(
    int Id,
    string Descripcion,
    string TipoCobro,
    decimal Precio
);
