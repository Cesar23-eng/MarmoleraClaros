import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';

/// FinanzasDashboard — Núcleo de control de márgenes y auditoría de ingresos.
/// Este será el dashboard más complejo del ERP, con análisis financiero,
/// auditoría de ingresos y control de márgenes por producto/proyecto.
class FinanzasDashboard extends StatelessWidget {
  const FinanzasDashboard({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.backgroundWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.primaryBlue,
        title: const Text(
          'Panel de Finanzas',
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.w700),
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout, color: Colors.white70),
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
        ],
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.account_balance_outlined, size: 72, color: AppTheme.primaryBlue),
            SizedBox(height: 20),
            Text(
              'Dashboard de Finanzas',
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.w700,
                color: AppTheme.textPrimary,
              ),
            ),
            SizedBox(height: 8),
            Text(
              'Control de Márgenes · Auditoría de Ingresos',
              style: TextStyle(fontSize: 14, color: AppTheme.primaryBlue),
            ),
            SizedBox(height: 8),
            Text(
              'Módulo en construcción',
              style: TextStyle(fontSize: 13, color: AppTheme.textSecondary),
            ),
          ],
        ),
      ),
    );
  }
}
