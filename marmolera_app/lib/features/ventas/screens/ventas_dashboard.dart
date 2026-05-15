import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/features/ventas/screens/detalles_cotizacion_screen.dart';
import 'package:marmolera_app/features/ventas/screens/historial_cotizaciones_screen.dart';
import 'package:marmolera_app/features/ventas/screens/registro_cliente_screen.dart';
import 'package:marmolera_app/features/ventas/services/clientes_service.dart';

/// Pantalla 1 del flujo de ventas: selección del cliente.
/// Al confirmar navega a [DetallesCotizacionScreen] pasando el [ClienteDto].
class VentasDashboard extends StatefulWidget {
  const VentasDashboard({super.key});

  @override
  State<VentasDashboard> createState() => _VentasDashboardState();
}

class _VentasDashboardState extends State<VentasDashboard> {
  // ── Estado del cliente seleccionado ───────────────────────────────────────
  ClienteDto? _clienteSeleccionado;
  final _searchCtrl = TextEditingController();

  final ClientesService _clientesService = ClientesService();

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  // ── Ir a registrar cliente nuevo ──────────────────────────────────────────
  Future<void> _irARegistro() async {
    final nuevo = await Navigator.push<ClienteDto>(
      context,
      MaterialPageRoute(builder: (_) => const RegistroClienteScreen()),
    );
    if (nuevo != null && mounted) {
      setState(() {
        _clienteSeleccionado = nuevo;
        _searchCtrl.text = nuevo.nombre;
      });
    }
  }

  // ── Continuar a Detalles del Mesón ────────────────────────────────────────
  void _continuar() {
    if (_clienteSeleccionado == null) return;
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) =>
            DetallesCotizacionScreen(cliente: _clienteSeleccionado!),
      ),
    );
  }

  // ── Widget de búsqueda Autocomplete ───────────────────────────────────────
  Widget _buildBuscador() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Autocomplete
        Autocomplete<ClienteDto>(
          displayStringForOption: (c) => c.nombre,
          optionsBuilder: (text) async {
            if (text.text.trim().length < 2) return [];
            try {
              return await _clientesService.buscarClientes(text.text);
            } catch (_) {
              return [];
            }
          },
          fieldViewBuilder: (ctx, ctrl, focusNode, onSubmitted) {
            // Sincroniza el controlador externo para poder limpiar
            if (_clienteSeleccionado == null && _searchCtrl.text.isEmpty) {
              ctrl.clear();
            }
            return TextFormField(
              controller: ctrl,
              focusNode: focusNode,
              onChanged: (_) {
                // Si el usuario escribe de nuevo, deseleccionar cliente
                if (_clienteSeleccionado != null) {
                  setState(() => _clienteSeleccionado = null);
                }
              },
              style:
                  const TextStyle(color: AppTheme.textPrimary, fontSize: 15),
              decoration: InputDecoration(
                hintText: 'Escribe el nombre del cliente…',
                hintStyle: const TextStyle(
                    color: AppTheme.textSecondary, fontSize: 13),
                prefixIcon: const Icon(Icons.search_rounded, size: 20),
                suffixIcon: _clienteSeleccionado != null
                    ? const Icon(Icons.check_circle_rounded,
                        color: Color(0xFF27AE60), size: 20)
                    : null,
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12)),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(
                    color: _clienteSeleccionado != null
                        ? const Color(0xFF27AE60)
                        : AppTheme.primaryBlue.withValues(alpha: 0.5),
                  ),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(
                      color: AppTheme.primaryBlue, width: 1.8),
                ),
                filled: true,
                fillColor: AppTheme.backgroundWhite.withValues(alpha: 0.5),
              ),
              onFieldSubmitted: (_) => onSubmitted(),
            );
          },
          optionsViewBuilder: (ctx, onSelected, options) {
            return Align(
              alignment: Alignment.topLeft,
              child: Material(
                color: AppTheme.cardColor,
                elevation: 8,
                borderRadius: BorderRadius.circular(12),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxHeight: 240),
                  child: ListView.separated(
                    padding: const EdgeInsets.symmetric(vertical: 6),
                    shrinkWrap: true,
                    itemCount: options.length,
                    separatorBuilder: (_, __) => Divider(
                      height: 1,
                      color: AppTheme.primaryBlue.withValues(alpha: 0.2),
                    ),
                    itemBuilder: (_, i) {
                      final c = options.elementAt(i);
                      return ListTile(
                        dense: true,
                        leading: const Icon(Icons.person_outline,
                            color: AppTheme.primaryBlue, size: 20),
                        title: Text(
                          c.nombre,
                          style: const TextStyle(
                              color: AppTheme.textPrimary,
                              fontSize: 14,
                              fontWeight: FontWeight.w500),
                        ),
                        subtitle: Text(
                          c.telefonoCliente,
                          style: const TextStyle(
                              color: AppTheme.textSecondary, fontSize: 12),
                        ),
                        onTap: () => onSelected(c),
                      );
                    },
                  ),
                ),
              ),
            );
          },
          onSelected: (c) => setState(() {
            _clienteSeleccionado = c;
            _searchCtrl.text = c.nombre;
          }),
        ),

        // Chip del cliente seleccionado
        if (_clienteSeleccionado != null) ...[
          const SizedBox(height: 12),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: BoxDecoration(
              color: const Color(0xFF27AE60).withValues(alpha: 0.08),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(
                  color: const Color(0xFF27AE60).withValues(alpha: 0.3)),
            ),
            child: Row(
              children: [
                const Icon(Icons.person_pin_rounded,
                    size: 18, color: Color(0xFF27AE60)),
                const SizedBox(width: 10),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        _clienteSeleccionado!.nombre,
                        style: const TextStyle(
                          color: AppTheme.textPrimary,
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      Text(
                        '📞 ${_clienteSeleccionado!.telefonoCliente}  ·  📍 ${_clienteSeleccionado!.direccionCliente}',
                        style: const TextStyle(
                            color: AppTheme.textSecondary, fontSize: 12),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ),
                ),
                IconButton(
                  icon: const Icon(Icons.close_rounded,
                      size: 18, color: AppTheme.textSecondary),
                  onPressed: () => setState(() {
                    _clienteSeleccionado = null;
                    _searchCtrl.clear();
                  }),
                  tooltip: 'Cambiar cliente',
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(),
                ),
              ],
            ),
          ),
        ],

        const SizedBox(height: 16),

        // Botón registrar nuevo cliente
        TextButton.icon(
          onPressed: _irARegistro,
          icon: const Icon(Icons.person_add_alt_1_rounded,
              size: 16, color: AppTheme.primaryBlue),
          label: const Text(
            'Registrar cliente nuevo',
            style: TextStyle(
              color: AppTheme.primaryBlue,
              fontSize: 13,
              fontWeight: FontWeight.w600,
            ),
          ),
          style: TextButton.styleFrom(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
              side: BorderSide(
                  color: AppTheme.primaryBlue.withValues(alpha: 0.35)),
            ),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final clienteOk = _clienteSeleccionado != null;

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
              'Panel de Ventas',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              'Paso 1 · Selección del Cliente',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
        actions: [
          // Historial de cotizaciones
          IconButton(
            icon: const Icon(Icons.list_alt_rounded,
                color: Colors.white),
            tooltip: 'Historial de cotizaciones',
            onPressed: () => Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => const HistorialCotizacionesScreen(),
              ),
            ),
          ),
          // Cerrar sesión
          IconButton(
            icon: const Icon(Icons.logout_rounded,
                color: Colors.white70),
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
            const SizedBox(height: 4),
            // ── Encabezado ──────────────────────────────────────────────────
            Text(
              'Nueva Cotización',
              style: theme.textTheme.headlineSmall?.copyWith(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Primero, selecciona o registra al cliente para continuar.',
              style: theme.textTheme.bodySmall
                  ?.copyWith(color: AppTheme.textSecondary),
            ),
            const SizedBox(height: 28),

            // ── Indicador de progreso de pasos ───────────────────────────────
            _buildStepIndicator(theme),
            const SizedBox(height: 28),

            // ╔══════════════════════════════════════════════════════╗
            // ║  CARD: Seleccionar Cliente                           ║
            // ╚══════════════════════════════════════════════════════╝
            Card(
              elevation: 4,
              color: AppTheme.cardColor,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(18),
                side: BorderSide(
                  color: clienteOk
                      ? const Color(0xFF27AE60).withValues(alpha: 0.5)
                      : AppTheme.primaryBlue.withValues(alpha: 0.35),
                ),
              ),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Container(
                          width: 36,
                          height: 36,
                          decoration: BoxDecoration(
                            color: clienteOk
                                ? const Color(0xFF27AE60)
                                    .withValues(alpha: 0.12)
                                : AppTheme.primaryBlue.withValues(alpha: 0.12),
                            borderRadius: BorderRadius.circular(9),
                          ),
                          child: Icon(
                            clienteOk
                                ? Icons.person_pin_rounded
                                : Icons.person_search_rounded,
                            color: clienteOk
                                ? const Color(0xFF27AE60)
                                : AppTheme.primaryBlue,
                            size: 20,
                          ),
                        ),
                        const SizedBox(width: 12),
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Seleccionar Cliente',
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: AppTheme.textPrimary,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                            Text(
                              'Busca por nombre o registra uno nuevo',
                              style: theme.textTheme.labelSmall?.copyWith(
                                color: AppTheme.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),
                    Divider(
                      color: AppTheme.primaryBlue.withValues(alpha: 0.4),
                      thickness: 1,
                    ),
                    const SizedBox(height: 20),
                    _buildBuscador(),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 28),

            // ── Botón Continuar ─────────────────────────────────────────────
            AnimatedOpacity(
              opacity: clienteOk ? 1.0 : 0.4,
              duration: const Duration(milliseconds: 250),
              child: SizedBox(
                width: double.infinity,
                height: 56,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primaryBlue,
                    foregroundColor: AppTheme.backgroundWhite,
                    elevation: clienteOk ? 8 : 0,
                    shadowColor:
                        AppTheme.primaryBlue.withValues(alpha: 0.4),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                  ),
                  onPressed: clienteOk ? _continuar : null,
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.arrow_forward_rounded, size: 22),
                      const SizedBox(width: 10),
                      Text(
                        'CONTINUAR A DETALLES DEL MESÓN',
                        style: theme.textTheme.labelLarge?.copyWith(
                          fontWeight: FontWeight.w800,
                          letterSpacing: 0.8,
                          color: AppTheme.backgroundWhite,
                          fontSize: 14,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const SizedBox(height: 8),

            // Hint cuando no hay cliente
            if (!clienteOk)
              Center(
                child: Text(
                  'Selecciona un cliente para continuar',
                  style: theme.textTheme.labelSmall?.copyWith(
                    color: AppTheme.textSecondary,
                    fontStyle: FontStyle.italic,
                  ),
                ),
              ),
            const SizedBox(height: 16),
          ],
        ),
      ),
    );
  }

  // ── Indicador de pasos ───────────────────────────────────────────────────
  Widget _buildStepIndicator(ThemeData theme) {
    return Row(
      children: [
        // Paso 1 — activo
        _stepChip(
          number: '1',
          label: 'Cliente',
          active: true,
          done: _clienteSeleccionado != null,
          theme: theme,
        ),
        Expanded(
          child: Container(
            height: 2,
            margin: const EdgeInsets.symmetric(horizontal: 8),
            decoration: BoxDecoration(
              color: AppTheme.primaryBlue.withValues(alpha: 0.3),
              borderRadius: BorderRadius.circular(1),
            ),
          ),
        ),
        // Paso 2 — pendiente
        _stepChip(
          number: '2',
          label: 'Mesón',
          active: false,
          done: false,
          theme: theme,
        ),
      ],
    );
  }

  Widget _stepChip({
    required String number,
    required String label,
    required bool active,
    required bool done,
    required ThemeData theme,
  }) {
    final color = done
        ? const Color(0xFF27AE60)
        : active
            ? AppTheme.primaryBlue
            : AppTheme.textSecondary.withValues(alpha: 0.4);

    return Column(
      children: [
        CircleAvatar(
          radius: 16,
          backgroundColor: color.withValues(alpha: 0.15),
          child: done
              ? Icon(Icons.check_rounded, size: 16, color: color)
              : Text(
                  number,
                  style: TextStyle(
                    color: color,
                    fontWeight: FontWeight.w700,
                    fontSize: 13,
                  ),
                ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: theme.textTheme.labelSmall?.copyWith(
            color: color,
            fontWeight: active ? FontWeight.w600 : FontWeight.w400,
          ),
        ),
      ],
    );
  }
}
