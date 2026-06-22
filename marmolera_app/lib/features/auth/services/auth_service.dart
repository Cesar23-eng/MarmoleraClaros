import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

import 'package:marmolera_app/core/constants/app_constants.dart';
import 'package:marmolera_app/core/exceptions/auth_exception.dart';

class AuthService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  // ══════════════════════════════════════════════════════════════════════════
  //  Login simplificado: solo nombre de usuario (la app móvil/desktop usa este)
  //  El API agrega @marmolera.com automáticamente.
  // ══════════════════════════════════════════════════════════════════════════
  Future<String> login(String usuario, String password) async {
    return _loginRequest(
      endpoint: '${AppConstants.baseUrl}/api/auth/login-usuario',
      body: {'usuario': usuario.trim().toLowerCase(), 'password': password},
    );
  }

  // ══════════════════════════════════════════════════════════════════════════
  //  Login clásico con email completo (para Swagger / uso Admin)
  // ══════════════════════════════════════════════════════════════════════════
  Future<String> loginConEmail(String email, String password) async {
    return _loginRequest(
      endpoint: AppConstants.loginEndpoint,
      body: {'email': email.trim(), 'password': password},
    );
  }

  // ── Helper interno compartido ──────────────────────────────────────────────
  Future<String> _loginRequest({
    required String endpoint,
    required Map<String, String> body,
  }) async {
    try {
      final response = await http
          .post(
            Uri.parse(endpoint),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode(body),
          )
          .timeout(
            const Duration(seconds: 15),
            onTimeout: () => throw AuthException(
              'Tiempo de espera agotado. Verifica tu conexión.',
            ),
          );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as Map<String, dynamic>;

        // Guardar accessToken
        final token = (data['accessToken'] ?? data['token']) as String?;
        if (token == null || token.isEmpty) {
          throw AuthException('El servidor no devolvió un token válido.');
        }
        await _storage.write(key: AppConstants.tokenKey, value: token);

        // Guardar refreshToken si viene
        final refresh = data['refreshToken'] as String?;
        if (refresh != null && refresh.isNotEmpty) {
          await _storage.write(key: 'refreshToken', value: refresh);
        }

        // Extraer rol del JWT
        final claims = JwtDecoder.decode(token);
        final role = claims['role'] ??
            claims['Role'] ??
            claims[
                'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
            '';

        if (role.toString().isEmpty) {
          throw AuthException('No se encontró el rol del usuario en el token.');
        }

        return role.toString();
      } else if (response.statusCode == 401) {
        // Leer mensaje de la API si viene
        try {
          final err = jsonDecode(response.body) as Map<String, dynamic>;
          throw AuthException(err['mensaje']?.toString() ?? 'Credenciales incorrectas.');
        } catch (e) {
          if (e is AuthException) rethrow;
          throw AuthException('Usuario o contraseña incorrectos.');
        }
      } else {
        throw AuthException(
          'Error del servidor (${response.statusCode}). Intenta de nuevo.',
        );
      }
    } on AuthException {
      rethrow;
    } catch (e) {
      if (e is AuthException) rethrow;
      throw AuthException(
        'Error de conexión. Verifica que el servidor esté activo.',
      );
    }
  }

  /// Cierra sesión: elimina tokens del storage seguro.
  Future<void> logout() async {
    await _storage.delete(key: AppConstants.tokenKey);
    await _storage.delete(key: 'refreshToken');
  }

  /// Recupera el accessToken guardado.
  Future<String?> getToken() async {
    return await _storage.read(key: AppConstants.tokenKey);
  }

  /// Verifica si existe un token activo y no expirado.
  Future<bool> isAuthenticated() async {
    final token = await getToken();
    if (token == null) return false;
    return !JwtDecoder.isExpired(token);
  }
}
