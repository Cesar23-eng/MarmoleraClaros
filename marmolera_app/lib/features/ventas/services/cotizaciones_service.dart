import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

import 'package:marmolera_app/core/constants/app_constants.dart';
import 'package:marmolera_app/features/fabrica/models/pedido_produccion.dart';

/// Servicio para interactuar con el endpoint de cotizaciones del backend .NET.
/// Soporta arquitectura Cabecera-Detalle (una cotización → múltiples mesones).
class CotizacionesService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  static const String _endpoint = '${AppConstants.baseUrl}/cotizaciones';

  // ── Helper: token o excepción ──────────────────────────────────────────────
  Future<String> _getToken() async {
    final t = await _storage.read(key: AppConstants.tokenKey);
    if (t == null || t.isEmpty) {
      throw Exception('No hay sesión activa. Por favor inicia sesión.');
    }
    return t;
  }

  // ── Helper: headers autenticados ───────────────────────────────────────────
  Future<Map<String, String>> _headers() async => {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ${await _getToken()}',
      };

  // ── Helper: interpretar errores HTTP ──────────────────────────────────────
  void _checkStatus(http.Response response) {
    if (response.statusCode == 200 || response.statusCode == 201) return;
    if (response.statusCode == 401) {
      throw Exception('Sesión expirada. Por favor inicia sesión de nuevo.');
    }
    if (response.statusCode == 400) {
      try {
        final body = jsonDecode(response.body) as Map<String, dynamic>;
        // ASP.NET devuelve errores de validación en body['errors']
        if (body.containsKey('errors')) {
          final errors = (body['errors'] as Map<String, dynamic>)
              .values
              .expand((v) => v as List)
              .join(', ');
          throw Exception('Validación: $errors');
        }
        throw Exception(
            body['mensaje'] ?? body['message'] ?? body['title'] ?? 'Datos inválidos.');
      } on FormatException {
        // El body no es JSON — mostrarlo en crudo para ayudar al debug
        throw Exception('Error 400: ${response.body}');
      }
    }
    throw Exception('Error del servidor (${response.statusCode}).');
  }

  // ── 1. Crear cotización (Cabecera + Detalles) ──────────────────────────────
  /// Envía al backend una cotización con múltiples mesones.
  ///
  /// Estructura esperada de [datos]:
  /// ```json
  /// {
  ///   "clienteId": 5,
  ///   "comentarios": "...",
  ///   "detalles": [
  ///     {
  ///       "material": "Granito Chiquitano",
  ///       "geometria": 0,
  ///       "medidasJson": "{\"ladoA\":2.5,\"ladoB\":1.2}",
  ///       "areaTotal": 3.0,
  ///       "precioSubtotal": 900.0
  ///     }
  ///   ]
  /// }
  /// ```
  Future<Map<String, dynamic>> crearCotizacion(
    Map<String, dynamic> datos,
  ) async {
    final response = await http
        .post(
          Uri.parse(_endpoint),
          headers: await _headers(),
          body: jsonEncode(datos),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
    return jsonDecode(response.body) as Map<String, dynamic>;
  }

  // ── 2. Actualizar cotización ───────────────────────────────────────────────
  /// Hace un PUT a `/cotizaciones/{id}` para actualizar la cotización.
  Future<void> actualizarCotizacion(
    int id,
    Map<String, dynamic> data,
  ) async {
    final response = await http
        .put(
          Uri.parse('$_endpoint/$id'),
          headers: await _headers(),
          body: jsonEncode(data),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
  }

  // ── 3. Listar cotizaciones del vendedor ────────────────────────────────────
  /// Hace un GET a `/cotizaciones` y devuelve la lista completa.
  Future<List<dynamic>> obtenerCotizaciones() async {
    final response = await http
        .get(
          Uri.parse(_endpoint),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      return jsonDecode(response.body) as List<dynamic>;
    }
    _checkStatus(response);
    return [];
  }

  // ── 4. Aprobar cotización ────────────────────────────────────────────────────────
  /// Hace un PUT a `/cotizaciones/{id}/aprobar`.
  /// Cambia el estado de "Cotizado" → "Aprobado" y registra la fecha de aprobación.
  Future<void> aprobarCotizacion(int id) async {
    final response = await http
        .put(
          Uri.parse('$_endpoint/$id/aprobar'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
  }

  // ── 5. Obtener pedidos pendientes de producción ────────────────────────────
  /// Hace un GET a `/cotizaciones/pendientes-produccion`.
  /// Devuelve las cotizaciones con estado "Aprobado" para el tablero de fábrica.
  Future<List<PedidoProduccion>> obtenerPedidosProduccion() async {
    final response = await http
        .get(
          Uri.parse('$_endpoint/pendientes-produccion'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      final lista = jsonDecode(response.body) as List<dynamic>;
      return lista
          .map((j) => PedidoProduccion.fromJson(j as Map<String, dynamic>))
          .toList();
    }
    _checkStatus(response);
    return [];
  }

  // ── 6. Iniciar fabricación ─────────────────────────────────────────────────────────
  /// Hace un PUT a `/cotizaciones/{id}/iniciar-produccion`.
  /// Cambia el estado de "Aprobado" → "EnProduccion".
  Future<void> iniciarFabricacion(int id) async {
    final response = await http
        .put(
          Uri.parse('$_endpoint/$id/iniciar-produccion'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
  }
  // ── 7. Obtener pedidos en producción ────────────────────────────
  /// Hace un GET a `/cotizaciones/en-produccion`.
  /// Devuelve las cotizaciones con estado "EnProduccion".
  Future<List<PedidoProduccion>> obtenerEnProduccion() async {
    final response = await http
        .get(
          Uri.parse('$_endpoint/en-produccion'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      final lista = jsonDecode(response.body) as List<dynamic>;
      return lista
          .map((j) => PedidoProduccion.fromJson(j as Map<String, dynamic>))
          .toList();
    }
    _checkStatus(response);
    return [];
  }

  // ── 8. Obtener pedidos terminados ────────────────────────────
  /// Hace un GET a `/cotizaciones/terminados`.
  /// Devuelve las cotizaciones con estado "Finalizado".
  Future<List<PedidoProduccion>> obtenerTerminados() async {
    final response = await http
        .get(
          Uri.parse('$_endpoint/terminados'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      final lista = jsonDecode(response.body) as List<dynamic>;
      return lista
          .map((j) => PedidoProduccion.fromJson(j as Map<String, dynamic>))
          .toList();
    }
    _checkStatus(response);
    return [];
  }

  // ── 9. Finalizar producción ─────────────────────────────────────────────────────────
  /// Hace un PUT a `/cotizaciones/{id}/finalizar-produccion`.
  /// Cambia el estado de "EnProduccion" → "Finalizado".
  Future<void> finalizarProduccion(int id) async {
    final response = await http
        .put(
          Uri.parse('$_endpoint/$id/finalizar-produccion'),
          headers: await _headers(),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
  }
}
