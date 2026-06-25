namespace MarmoleraERP.API.Modules.Ventas.DTOs;

/// <summary>Body para aprobar cotización, con opción de agendar visita.</summary>
public record AprobarCotizacionDto(
    bool      RequiereMedicion,
    DateTime? FechaVisita,
    string?   NotasVisita
);

/// <summary>Body para que la arquitecta confirme o reprograme la visita.</summary>
public record ConfirmarVisitaDto(
    DateTime FechaConfirmada,
    string?  Motivo
);

/// <summary>Respuesta de una visita de medición pendiente.</summary>
public record VisitaResponseDto(
    int       EventoId,
    int       CotizacionId,
    string    ClienteNombre,
    string    ClienteTelefono,
    string    ClienteDireccion,
    DateTime  FechaVisita,
    string    EstadoVisita,
    string?   Notas,
    bool      FueReprogramada,
    DateTime? FechaOriginal
);
