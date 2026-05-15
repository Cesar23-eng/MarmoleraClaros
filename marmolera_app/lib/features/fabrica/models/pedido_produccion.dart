import 'dart:convert';

// ── Detalle (mesón individual) ────────────────────────────────────────────────

/// Representa un mesón dentro de una cotización aprobada.
/// Mapea la respuesta de [DetalleCotizacionResponseDto] del backend.
class DetalleCotizacion {
  final int id;
  final String nombreMaterial;
  final String geometria; // "Rectangulo" | "Forma_L" | "Forma_U"
  final String medidasJson; // JSON serializado con LadoA, LadoB, LadoC?, Ancho?
  final double precioPorM2;
  final double areaTotal;
  final double precioSubtotal;

  const DetalleCotizacion({
    required this.id,
    required this.nombreMaterial,
    required this.geometria,
    required this.medidasJson,
    required this.precioPorM2,
    required this.areaTotal,
    required this.precioSubtotal,
  });

  factory DetalleCotizacion.fromJson(Map<String, dynamic> json) {
    return DetalleCotizacion(
      id:             json['id']             as int,
      nombreMaterial: json['nombreMaterial'] as String? ?? '—',
      geometria:      json['geometria']      as String? ?? 'Rectangulo',
      medidasJson:    json['medidasJson']    as String? ?? '{}',
      precioPorM2:    (json['precioPorM2']   as num?)?.toDouble() ?? 0,
      areaTotal:      (json['areaTotal']     as num?)?.toDouble() ?? 0,
      precioSubtotal: (json['precioSubtotal'] as num?)?.toDouble() ?? 0,
    );
  }

  // ── Helpers de display ────────────────────────────────────────────────────

  /// Etiqueta legible de la geometría.
  String get geometriaLabel {
    switch (geometria) {
      case 'Forma_L': return 'Forma L';
      case 'Forma_U': return 'Forma U';
      default:        return 'Rectángulo';
    }
  }

  /// Medidas parseadas desde el JSON almacenado.
  String get medidasLabel {
    try {
      final m = jsonDecode(medidasJson) as Map<String, dynamic>;
      final a  = (m['LadoA'] as num?)?.toStringAsFixed(2) ?? '?';
      final b  = (m['LadoB'] as num?)?.toStringAsFixed(2) ?? '?';
      final c  = m['LadoC']  != null ? (m['LadoC']  as num).toStringAsFixed(2) : null;
      final an = m['Ancho']  != null ? (m['Ancho']  as num).toStringAsFixed(2) : null;
      switch (geometria) {
        case 'Forma_L': return 'L: $a × $b × ${an ?? '?'} m';
        case 'Forma_U': return 'U: $a × $b × ${c ?? '?'} × ${an ?? '?'} m';
        default:        return '$a × $b m';
      }
    } catch (_) {
      return medidasJson;
    }
  }
}

// ── Cabecera (cotización completa) ────────────────────────────────────────────

/// Representa una cotización aprobada devuelta por:
///   GET /api/cotizaciones/pendientes-produccion
/// Mapea [CotizacionResponseDto] del backend.
class PedidoProduccion {
  final int                    id;
  final String                 estado;
  final double                 precioTotal;
  final DateTime               fechaCreacion;
  final DateTime?              fechaAprobacion;
  final String?                comentarios;

  // Cliente embebido
  final int                    clienteId;
  final String                 nombreCliente;

  // Lista de mesones
  final List<DetalleCotizacion> detalles;

  const PedidoProduccion({
    required this.id,
    required this.estado,
    required this.precioTotal,
    required this.fechaCreacion,
    this.fechaAprobacion,
    this.comentarios,
    required this.clienteId,
    required this.nombreCliente,
    required this.detalles,
  });

  factory PedidoProduccion.fromJson(Map<String, dynamic> json) {
    final cliente = json['cliente'] as Map<String, dynamic>? ?? {};
    final rawDetalles = json['detalles'] as List<dynamic>? ?? [];

    return PedidoProduccion(
      id:              json['id']          as int,
      estado:          json['estado']      as String? ?? 'Aprobado',
      precioTotal:     (json['precioTotal'] as num?)?.toDouble() ?? 0,
      fechaCreacion:   DateTime.tryParse(json['fechaCreacion']  as String? ?? '') ?? DateTime.now(),
      fechaAprobacion: json['fechaAprobacion'] != null
          ? DateTime.tryParse(json['fechaAprobacion'] as String)
          : null,
      comentarios:     json['comentarios'] as String?,
      clienteId:       cliente['id']             as int?  ?? 0,
      nombreCliente:   cliente['nombreCompleto']  as String? ?? 'Cliente desconocido',
      detalles: rawDetalles
          .map((d) => DetalleCotizacion.fromJson(d as Map<String, dynamic>))
          .toList(),
    );
  }

  /// Área total sumando todos los mesones.
  double get areaTotal =>
      detalles.fold(0, (sum, d) => sum + d.areaTotal);

  /// Inicial del nombre del cliente para el avatar.
  String get inicialCliente =>
      nombreCliente.isNotEmpty ? nombreCliente[0].toUpperCase() : '?';
}

