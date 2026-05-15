import 'package:flutter/material.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';
import 'package:marmolera_app/features/ventas/screens/ventas_dashboard.dart';
import 'package:marmolera_app/features/fabrica/screens/fabrica_dashboard.dart';
import 'package:marmolera_app/features/dashboards/finanzas_dashboard.dart';
import 'package:marmolera_app/features/dashboards/gerencia_dashboard.dart';
import 'package:marmolera_app/features/ordenes/screens/tablet_ordenes_screen.dart';
import 'package:marmolera_app/features/calendario/screens/calendario_screen.dart';
import 'package:marmolera_app/features/notificaciones/screens/notificaciones_screen.dart';
import 'package:marmolera_app/features/reportes/screens/reportes_screen.dart';

/// Rutas nombradas del sistema Marmolera Claros.
/// Cada rol tiene su ruta principal. Las sub-pantallas se acceden
/// por navegacion push directa desde sus respectivos dashboards.
class AppRoutes {
  AppRoutes._();

  // ── Rutas principales ───────────────────────────────────────────────
  static const String login       = '/login';
  static const String ventas      = '/ventas';
  static const String fabrica     = '/fabrica';
  static const String finanzas    = '/finanzas';
  static const String gerencia    = '/gerencia';
  static const String tablet      = '/tablet';

  // ── Sub-rutas compartidas ──────────────────────────────────────────
  static const String calendario      = '/calendario';
  static const String notificaciones  = '/notificaciones';
  static const String reportes        = '/reportes';

  // ── Map de rutas ──────────────────────────────────────────────────
  static Map<String, WidgetBuilder> get routes => {
    login:          (_) => const LoginScreen(),
    ventas:         (_) => const VentasDashboard(),
    fabrica:        (_) => const FabricaDashboard(),
    finanzas:       (_) => const FinanzasDashboard(),
    gerencia:       (_) => const GerenciaDashboard(),
    tablet:         (_) => const TabletOrdenesScreen(),
    calendario:     (_) => const CalendarioScreen(),
    notificaciones: (_) => const NotificacionesScreen(),
    reportes:       (_) => const ReportesScreen(),
  };

  /// Navega reemplazando toda la pila (usar post-login).
  static Future<void> goTo(BuildContext context, String route) =>
      Navigator.pushReplacementNamed(context, route);

  /// Navega a una sub-pantalla empujando sobre la pila actual.
  static Future<void> push(BuildContext context, String route) =>
      Navigator.pushNamed(context, route);
}
