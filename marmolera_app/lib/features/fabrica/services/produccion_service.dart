import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

import 'package:marmolera_app/core/constants/app_constants.dart';
import 'package:marmolera_app/features/fabrica/models/pedido_produccion.dart';

/// Servicio para interactuar con el módulo de Producción del backend .NET.
///
/// Endpoints:
///   GET  /api/produccion/pedidos           → Lista pedidos en fábrica
///   PUT  /api/produccion/pedidos/{id}/estado → Actualiza el estado Kanban
class ProduccionService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  static const String _base = '${AppConstants.baseUrl}/produccion';

  // ── Helper: obtener token o lanzar excepción ───────────────────────────────

  Future<String> _getToken() async {
    final token = await _storage.read(key: AppConstants.tokenKey);
    if (token == null || token.isEmpty) {
      throw Exception('No hay sesión activa. Por favor inicia sesión.');
    }
    return token;
  }

  Map<String, String> _headers(String token) => {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token',
      };

  // ── Obtener todos los pedidos en producción ────────────────────────────────

  /// Llama a GET /api/produccion/pedidos y devuelve la lista de pedidos
  /// que están en algún estado del tablero Kanban (Estado ≠ "Cotizado").
  Future<List<PedidoProduccion>> obtenerPedidos() async {
    final token = await _getToken();

    final response = await http
        .get(
          Uri.parse('$_base/pedidos'),
          headers: _headers(token),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      final lista = jsonDecode(response.body) as List<dynamic>;
      return lista
          .map((json) => PedidoProduccion.fromJson(json as Map<String, dynamic>))
          .toList();
    } else if (response.statusCode == 401) {
      throw Exception('Sesión expirada. Por favor inicia sesión de nuevo.');
    } else {
      throw Exception('Error al obtener pedidos (${response.statusCode}).');
    }
  }

  // ── Actualizar estado de un pedido ────────────────────────────────────────

  /// Llama a PUT /api/produccion/pedidos/{id}/estado con el nuevo estado.
  ///
  /// [pedidoId] — ID de la cotización.
  /// [nuevoEstado] — Uno de: "Pendiente", "EnCorte", "Pulido", "Terminado".
  ///
  /// Lanza [Exception] si el servidor responde con error.
  Future<void> actualizarEstado(int pedidoId, String nuevoEstado) async {
    final token = await _getToken();

    final response = await http
        .put(
          Uri.parse('$_base/pedidos/$pedidoId/estado'),
          headers: _headers(token),
          body: jsonEncode({'nuevoEstado': nuevoEstado}),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    if (response.statusCode == 200) {
      return; // OK
    } else if (response.statusCode == 404) {
      throw Exception('Pedido #$pedidoId no encontrado en el servidor.');
    } else if (response.statusCode == 401) {
      throw Exception('Sesión expirada. Por favor inicia sesión de nuevo.');
    } else {
      throw Exception('Error al actualizar estado (${response.statusCode}).');
    }
  }
}
