import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

import 'package:marmolera_app/core/constants/app_constants.dart';

// ─── Modelo liviano de cliente ────────────────────────────────────────────────

class ClienteDto {
  final int id;
  final String nombre;
  final String telefonoCliente;
  final String direccionCliente;

  const ClienteDto({
    required this.id,
    required this.nombre,
    required this.telefonoCliente,
    required this.direccionCliente,
  });

  factory ClienteDto.fromJson(Map<String, dynamic> json) => ClienteDto(
        id: json['id'] as int,
        nombre: json['nombreCompleto'] as String,
        telefonoCliente: (json['telefono'] ?? '') as String,
        direccionCliente: (json['direccion'] ?? '') as String,
      );

  @override
  String toString() => nombre; // usado por Autocomplete
}

// ─── Servicio ─────────────────────────────────────────────────────────────────

/// Servicio para interactuar con el endpoint `/clientes` del backend .NET.
class ClientesService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  static const String _endpoint = '${AppConstants.baseUrl}/clientes';

  // ── Helper: obtener token o lanzar excepción ────────────────────────────────
  Future<String> _token() async {
    final t = await _storage.read(key: AppConstants.tokenKey);
    if (t == null || t.isEmpty) {
      throw Exception('No hay sesión activa. Por favor inicia sesión.');
    }
    return t;
  }

  // ── Helper: headers base ───────────────────────────────────────────────────
  Future<Map<String, String>> _headers() async => {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ${await _token()}',
      };

  // ── Helper: manejar errores HTTP comunes ───────────────────────────────────
  void _checkStatus(http.Response response) {
    if (response.statusCode == 200 || response.statusCode == 201) return;
    if (response.statusCode == 401) {
      throw Exception('Sesión expirada. Por favor inicia sesión de nuevo.');
    }
    if (response.statusCode == 400) {
      try {
        final body = jsonDecode(response.body) as Map<String, dynamic>;
        final msg = body['mensaje'] ?? body['message'] ?? 'Datos inválidos.';
        throw Exception(msg);
      } catch (_) {
        throw Exception('Solicitud inválida (400).');
      }
    }
    throw Exception('Error del servidor (${response.statusCode}).');
  }

  // ── 1. Registrar cliente ───────────────────────────────────────────────────
  /// Envía un POST con los datos del cliente y devuelve el [ClienteDto]
  /// creado por el servidor (incluye el ID asignado).
  Future<ClienteDto> registrarCliente({
    required String nombre,
    required String telefonoCliente,
    required String direccionCliente,
  }) async {
    final response = await http
        .post(
          Uri.parse(_endpoint),
          headers: await _headers(),
          body: jsonEncode({
            'nombreCompleto': nombre,
            'telefono': telefonoCliente,
            'direccion': direccionCliente,
          }),
        )
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);
    return ClienteDto.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }

  // ── 2. Buscar clientes por nombre ─────────────────────────────────────────
  /// Llama al GET `/clientes/buscar?nombre=<query>` y devuelve la lista.
  /// Devuelve lista vacía si no hay resultados o si [query] tiene < 2 chars.
  Future<List<ClienteDto>> buscarClientes(String query) async {
    if (query.trim().length < 2) return [];

    final uri = Uri.parse(_endpoint).replace(
      path: '${Uri.parse(_endpoint).path}/buscar',
      queryParameters: {'nombre': query.trim()},
    );

    final response = await http
        .get(uri, headers: await _headers())
        .timeout(
          const Duration(seconds: 10),
          onTimeout: () =>
              throw Exception('Tiempo de espera agotado. Verifica tu conexión.'),
        );

    _checkStatus(response);

    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => ClienteDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
