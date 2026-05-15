import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

import 'package:marmolera_app/core/constants/app_constants.dart';
import 'package:marmolera_app/core/exceptions/auth_exception.dart';

class AuthService {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  /// Realiza login contra la API .NET y devuelve el rol del usuario.
  /// Lanza [AuthException] si las credenciales son inválidas o hay error de red.
  Future<String> login(String email, String password) async {
    try {
      final response = await http
          .post(
            Uri.parse(AppConstants.loginEndpoint),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'email': email, 'password': password}),
          )
          .timeout(
            const Duration(seconds: 15),
            onTimeout: () => throw AuthException(
              'Tiempo de espera agotado. Verifica tu conexión.',
            ),
          );

      if (response.statusCode == 200) {
        final body = jsonDecode(response.body) as Map<String, dynamic>;

        // La API debe devolver { "token": "eyJ..." }
        final token = body['token'] as String?;
        if (token == null || token.isEmpty) {
          throw AuthException('El servidor no devolvió un token válido.');
        }

        // Guardar token de forma segura en el dispositivo
        await _storage.write(key: AppConstants.tokenKey, value: token);

        // Decodificar claims del JWT
        final Map<String, dynamic> claims = JwtDecoder.decode(token);

        // Extraer el rol — ajusta el claim key según tu API
        final role = claims['role'] ??
            claims['Role'] ??
            claims[
                'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
            '';

        if (role.toString().isEmpty) {
          throw AuthException(
            'No se encontró el rol del usuario en el token.',
          );
        }

        return role.toString();
      } else if (response.statusCode == 401) {
        throw AuthException('Correo o contraseña incorrectos.');
      } else if (response.statusCode == 403) {
        throw AuthException('No tienes permisos para acceder al sistema.');
      } else {
        throw AuthException(
          'Error del servidor (${response.statusCode}). Intenta de nuevo.',
        );
      }
    } on AuthException {
      rethrow;
    } catch (e) {
      throw AuthException(
        'Error de conexión. Verifica que el servidor esté activo.',
      );
    }
  }

  /// Cierra sesión: elimina el token del storage seguro.
  Future<void> logout() async {
    await _storage.delete(key: AppConstants.tokenKey);
  }

  /// Recupera el token guardado, o null si no hay sesión activa.
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
