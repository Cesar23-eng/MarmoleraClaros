import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/core/routes/app_routes.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';

/// Dashboard de Gerencia – acceso total al sistema.
/// Desde aquí se navega a todos los módulos.
class GerenciaDashboard extends StatelessWidget {
  const GerenciaDashboard({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.backgroundWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.primaryBlue,
        elevation: 0,
        titleSpacing: 16,
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Gerencia',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: Colors.white, fontWeight: FontWeight.w700),
            ),
            Text(
              'Vista completa del sistema',
              style: Theme.of(context).textTheme.labelSmall?.copyWith(
                    color: Colors.white70, letterSpacing: 0.5),
            ),
          ],
        ),
        actions: [
          // Notificaciones
          IconButton(
            icon: const Icon(Icons.notifications_outlined, color: Colors.white),
            tooltip: 'Notificaciones',
            onPressed: () => AppRoutes.push(context, AppRoutes.notificaciones),
          ),
          // Cerrar sesión
          IconButton(
            icon: const Icon(Icons.logout_rounded, color: Colors.white70),
            tooltip: 'Cerrar sesión',
            onPressed: () async {
              await AuthService().logout();
              if (context.mounted) {
                await Navigator.pushReplacementNamed(
                    context, AppRoutes.login);
              }
            },
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(16, 24, 16, 32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Módulos del sistema',
              style: TextStyle(
                  color: AppTheme.textPrimary,
                  fontSize: 18,
                  fontWeight: FontWeight.w800),
            ),
            const SizedBox(height: 4),
            Text(
              'Acceso completo a todas las áreas',
              style: TextStyle(
                  color: AppTheme.textSecondary.withValues(alpha: 0.8),
                  fontSize: 13),
            ),
            const SizedBox(height: 24),

            // Grid de módulos
            GridView.count(
              crossAxisCount: 2,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              mainAxisSpacing: 14,
              crossAxisSpacing: 14,
              childAspectRatio: 1.2,
              children: [
                _moduloCard(
                  context,
                  icono: Icons.point_of_sale_rounded,
                  titulo: 'Ventas',
                  subtitulo: 'Cotizaciones y pedidos',
                  color: AppTheme.primaryBlue,
                  ruta: AppRoutes.ventas,
                ),
                _moduloCard(
                  context,
                  icono: Icons.precision_manufacturing_rounded,
                  titulo: 'Fábrica',
                  subtitulo: 'Producción y estados',
                  color: const Color(0xFF2196F3),
                  ruta: AppRoutes.fabrica,
                ),
                _moduloCard(
                  context,
                  icono: Icons.account_balance_wallet_rounded,
                  titulo: 'Contabilidad',
                  subtitulo: 'Pagos y deudores',
                  color: const Color(0xFF27AE60),
                  ruta: AppRoutes.finanzas,
                ),
                _moduloCard(
                  context,
                  icono: Icons.camera_alt_rounded,
                  titulo: 'Tablet / Órdenes',
                  subtitulo: 'Subir órdenes escaneadas',
                  color: const Color(0xFFFFC107),
                  ruta: AppRoutes.tablet,
                ),
                _moduloCard(
                  context,
                  icono: Icons.calendar_month_rounded,
                  titulo: 'Calendario',
                  subtitulo: 'Entregas y medidas',
                  color: const Color(0xFF9C27B0),
                  ruta: AppRoutes.calendario,
                ),
                _moduloCard(
                  context,
                  icono: Icons.bar_chart_rounded,
                  titulo: 'Reportes',
                  subtitulo: 'Métricas y KPIs',
                  color: const Color(0xFFE53935),
                  ruta: AppRoutes.reportes,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _moduloCard(
    BuildContext context, {
    required IconData icono,
    required String titulo,
    required String subtitulo,
    required Color color,
    required String ruta,
  }) {
    return GestureDetector(
      onTap: () => AppRoutes.push(context, ruta),
      child: Container(
        decoration: BoxDecoration(
          color: AppTheme.cardColor,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
              color: color.withValues(alpha: 0.2), width: 1),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withValues(alpha: 0.04),
                blurRadius: 10,
                offset: const Offset(0, 4))
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 46,
                height: 46,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Center(
                  child: Icon(icono, color: color, size: 24),
                ),
              ),
              const SizedBox(height: 12),
              Text(titulo,
                  style: const TextStyle(
                      color: AppTheme.textPrimary,
                      fontWeight: FontWeight.w700,
                      fontSize: 14)),
              const SizedBox(height: 3),
              Text(subtitulo,
                  style: TextStyle(
                      color: AppTheme.textSecondary.withValues(alpha: 0.8),
                      fontSize: 11),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis),
            ],
          ),
        ),
      ),
    );
  }
}
