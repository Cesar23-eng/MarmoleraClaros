import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/features/ventas/screens/clientes_screen.dart';
import 'package:marmolera_app/features/ventas/screens/historial_cotizaciones_screen.dart';
import 'package:marmolera_app/features/ventas/screens/ventas_dashboard.dart'
    as nueva_cotizacion;

class VentasDashboard extends StatelessWidget {
  const VentasDashboard({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: AppTheme.backgroundWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.primaryBlue,
        foregroundColor: Colors.white,
        elevation: 0,
        titleSpacing: 16,
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'MarmoleraClaros',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              'Panel de Ventas',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded, color: Colors.white70),
            tooltip: 'Cerrar sesión',
            onPressed: () async {
              await AuthService().logout();
              if (context.mounted) {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(builder: (_) => const LoginScreen()),
                );
              }
            },
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 8),

            // ── Bienvenida ────────────────────────────────────────────────────
            Text(
              'Bienvenido',
              style: theme.textTheme.headlineSmall?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              '¿Qué deseas hacer hoy?',
              style: theme.textTheme.bodySmall
                  ?.copyWith(color: AppTheme.textSecondary),
            ),
            const SizedBox(height: 28),

            // ── Grid de accesos rápidos ───────────────────────────────────────
            GridView.count(
              crossAxisCount: 2,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              mainAxisSpacing: 16,
              crossAxisSpacing: 16,
              childAspectRatio: 1.1,
              children: [
                _MenuCard(
                  icon: Icons.add_circle_outline_rounded,
                  label: 'Nueva\nCotización',
                  color: AppTheme.primaryBlue,
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) =>
                          const nueva_cotizacion.VentasDashboard(),
                    ),
                  ),
                ),
                _MenuCard(
                  icon: Icons.people_alt_outlined,
                  label: 'Clientes',
                  color: const Color(0xFF27AE60),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const ClientesScreen(),
                    ),
                  ),
                ),
                _MenuCard(
                  icon: Icons.list_alt_rounded,
                  label: 'Historial de\nCotizaciones',
                  color: const Color(0xFFE67E22),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const HistorialCotizacionesScreen(),
                    ),
                  ),
                ),
                _MenuCard(
                  icon: Icons.bar_chart_rounded,
                  label: 'Reportes',
                  color: const Color(0xFF8E44AD),
                  onTap: () => ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Módulo de Reportes próximamente'),
                      behavior: SnackBarBehavior.floating,
                    ),
                  ),
                ),
              ],
            ),

            const SizedBox(height: 32),

            // ── Acceso rápido: Nueva Cotización ───────────────────────────────
            SizedBox(
              width: double.infinity,
              height: 56,
              child: ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primaryBlue,
                  foregroundColor: Colors.white,
                  elevation: 6,
                  shadowColor: AppTheme.primaryBlue.withValues(alpha: 0.35),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
                icon: const Icon(Icons.add_rounded, size: 22),
                label: Text(
                  'NUEVA COTIZACIÓN',
                  style: theme.textTheme.labelLarge?.copyWith(
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.8,
                    color: Colors.white,
                    fontSize: 15,
                  ),
                ),
                onPressed: () => Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) =>
                        const nueva_cotizacion.VentasDashboard(),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ── Tarjeta de menú ─────────────────────────────────────────────────────────
class _MenuCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _MenuCard({
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(18),
      child: Container(
        decoration: BoxDecoration(
          color: AppTheme.cardColor,
          borderRadius: BorderRadius.circular(18),
          border: Border.all(
            color: color.withValues(alpha: 0.35),
          ),
          boxShadow: [
            BoxShadow(
              color: color.withValues(alpha: 0.08),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 48,
              height: 48,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(icon, color: color, size: 26),
            ),
            const SizedBox(height: 12),
            Text(
              label,
              textAlign: TextAlign.center,
              style: theme.textTheme.titleSmall?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w700,
                height: 1.3,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
