import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/ventas/services/cotizaciones_service.dart';

/// Pantalla de historial: lista todas las cotizaciones del vendedor autenticado.
class HistorialCotizacionesScreen extends StatefulWidget {
  const HistorialCotizacionesScreen({super.key});

  @override
  State<HistorialCotizacionesScreen> createState() =>
      _HistorialCotizacionesScreenState();
}

class _HistorialCotizacionesScreenState
    extends State<HistorialCotizacionesScreen> {
  final CotizacionesService _service = CotizacionesService();

  late Future<List<dynamic>> _futuro;
  final Map<int, bool> _aprobando = {};

  @override
  void initState() {
    super.initState();
    _futuro = _service.obtenerCotizaciones();
  }

  void _recargar() => setState(() {
        _futuro = _service.obtenerCotizaciones();
      });

  // ── Aprobar cotización ────────────────────────────────────────────────────
  Future<void> _aprobar(int id) async {
    setState(() => _aprobando[id] = true);
    try {
      await _service.aprobarCotizacion(id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: const Row(children: [
          Icon(Icons.check_circle_rounded, color: Colors.white, size: 18),
          SizedBox(width: 10),
          Text('¡Cotización aprobada y enviada a Fábrica!',
              style: TextStyle(fontWeight: FontWeight.w600)),
        ]),
        backgroundColor: const Color(0xFF27AE60),
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        duration: const Duration(seconds: 3),
      ));
      _recargar();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Row(children: [
          const Icon(Icons.error_outline, color: Colors.white, size: 18),
          const SizedBox(width: 10),
          Expanded(
            child: Text(e.toString().replaceFirst('Exception: ', ''),
                overflow: TextOverflow.ellipsis),
          ),
        ]),
        backgroundColor: AppTheme.errorColor,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ));
    } finally {
      if (mounted) setState(() => _aprobando.remove(id));
    }
  }

  // ── Helpers de formato ────────────────────────────────────────────────────
  String _nombreCliente(Map<String, dynamic> json) {
    final cliente = json['cliente'] as Map<String, dynamic>?;
    return cliente?['nombreCompleto'] as String? ?? 'Cliente desconocido';
  }

  String _formatFecha(Map<String, dynamic> json) {
    final raw = json['fechaCreacion'] ?? json['fecha'] ?? json['createdAt'];
    if (raw == null) return 'Sin fecha';
    try {
      final dt = DateTime.parse(raw as String).toLocal();
      return DateFormat('dd/MM/yyyy  HH:mm').format(dt);
    } catch (_) {
      return raw.toString();
    }
  }

  String _estado(Map<String, dynamic> json) {
    return (json['estado'] ?? json['status'] ?? 'Desconocido').toString();
  }

  // ── Chip de estado ────────────────────────────────────────────────────────
  Widget _estadoChip(String estado) {
    Color bg;
    Color fg;
    IconData icon;

    switch (estado.toLowerCase()) {
      case 'cotizado':
        bg = AppTheme.primaryBlue.withValues(alpha: 0.18);
        fg = const Color(0xFF5DADE2);
        icon = Icons.description_outlined;
        break;
      case 'aprobado':
        bg = const Color(0xFF27AE60).withValues(alpha: 0.14);
        fg = const Color(0xFF27AE60);
        icon = Icons.verified_rounded;
        break;
      case 'en producción':
      case 'en produccion':
      case 'enproduccion':
        bg = const Color(0xFF8E44AD).withValues(alpha: 0.14);
        fg = const Color(0xFF8E44AD);
        icon = Icons.precision_manufacturing_rounded;
        break;
      case 'finalizado':
        bg = AppTheme.primaryBlue.withValues(alpha: 0.14);
        fg = AppTheme.primaryBlue;
        icon = Icons.emoji_events_rounded;
        break;
      case 'rechazado':
        bg = AppTheme.errorColor.withValues(alpha: 0.14);
        fg = AppTheme.errorColor;
        icon = Icons.cancel_outlined;
        break;
      default:
        bg = AppTheme.textSecondary.withValues(alpha: 0.12);
        fg = AppTheme.textSecondary;
        icon = Icons.info_outline_rounded;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: fg.withValues(alpha: 0.4)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: fg),
          const SizedBox(width: 5),
          Text(
            estado,
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w600,
              color: fg,
              letterSpacing: 0.3,
            ),
          ),
        ],
      ),
    );
  }

  // ── Tarjeta de cotización ─────────────────────────────────────────────────
  Widget _buildCard(Map<String, dynamic> json) {
    final id       = json['id'] as int? ?? 0;
    final nombre   = _nombreCliente(json);
    final fecha    = _formatFecha(json);
    final estado   = _estado(json);
    final area     = (json['areaTotal']   as num?)?.toStringAsFixed(2) ?? '—';
    final precio   = (json['precioTotal'] as num?)?.toStringAsFixed(2) ?? '—';
    final esCotizado    = estado.toLowerCase() == 'cotizado';
    final esAprobado    = estado.toLowerCase() == 'aprobado';
    final estaAprobando = _aprobando[id] ?? false;

    return Card(
      margin: const EdgeInsets.only(bottom: 14),
      elevation: 4,
      color: AppTheme.cardColor,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(
          color: AppTheme.primaryBlue.withValues(alpha: 0.25),
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.fromLTRB(18, 16, 18, 14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ── Fila superior: nombre + chip de estado ──────────────────────
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Avatar inicial
                Container(
                  width: 38,
                  height: 38,
                  decoration: BoxDecoration(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Center(
                    child: Text(
                      nombre.isNotEmpty ? nombre[0].toUpperCase() : '?',
                      style: const TextStyle(
                        color: AppTheme.primaryBlue,
                        fontSize: 16,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        nombre,
                        style: const TextStyle(
                          color: AppTheme.textPrimary,
                          fontSize: 15,
                          fontWeight: FontWeight.w700,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 2),
                      Text(
                        fecha,
                        style: const TextStyle(
                          color: AppTheme.textSecondary,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                _estadoChip(estado),
              ],
            ),

            const SizedBox(height: 14),
            Divider(
              color: AppTheme.primaryBlue.withValues(alpha: 0.25),
              height: 1,
            ),
            const SizedBox(height: 12),

            // ── Área y Precio ──────────────────────────────────────────────
            Row(
              children: [
                _metricTile(
                  icon: Icons.straighten_outlined,
                  label: 'Área total',
                  value: '$area m²',
                ),
                const SizedBox(width: 16),
                _metricTile(
                  icon: Icons.attach_money_rounded,
                  label: 'Precio total',
                  value: 'Bs $precio',
                  highlight: true,
                ),
              ],
            ),

            // ── Botón / Badge según el estado ──────────────────────────────
            if (esCotizado) ...[
              const SizedBox(height: 12),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: estaAprobando
                        ? const Color(0xFF27AE60).withValues(alpha: 0.15)
                        : const Color(0xFF27AE60),
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                    padding: const EdgeInsets.symmetric(vertical: 11),
                    elevation: 0,
                  ),
                  icon: estaAprobando
                      ? const SizedBox(
                          width: 15,
                          height: 15,
                          child: CircularProgressIndicator(
                              strokeWidth: 2, color: Colors.white),
                        )
                      : const Icon(Icons.thumb_up_alt_rounded, size: 16),
                  label: Text(
                    estaAprobando ? 'Aprobando…' : 'Aprobar Cotización',
                    style: const TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 0.3,
                    ),
                  ),
                  onPressed: estaAprobando ? null : () => _aprobar(id),
                ),
              ),
            ] else if (esAprobado) ...[
              const SizedBox(height: 12),
              Container(
                width: double.infinity,
                padding:
                    const EdgeInsets.symmetric(vertical: 9, horizontal: 12),
                decoration: BoxDecoration(
                  color: const Color(0xFF27AE60).withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(10),
                  border: Border.all(
                      color: const Color(0xFF27AE60).withValues(alpha: 0.35)),
                ),
                child: const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.factory_rounded,
                        size: 15, color: Color(0xFF27AE60)),
                    SizedBox(width: 8),
                    Text(
                      'Enviado a Fábrica',
                      style: TextStyle(
                        color: Color(0xFF27AE60),
                        fontSize: 13,
                        fontWeight: FontWeight.w700,
                        letterSpacing: 0.3,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _metricTile({
    required IconData icon,
    required String label,
    required String value,
    bool highlight = false,
  }) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          color: highlight
              ? AppTheme.primaryBlue.withValues(alpha: 0.07)
              : AppTheme.backgroundWhite.withValues(alpha: 0.5),
          borderRadius: BorderRadius.circular(10),
          border: Border.all(
            color: highlight
                ? AppTheme.primaryBlue.withValues(alpha: 0.2)
                : AppTheme.primaryBlue.withValues(alpha: 0.2),
          ),
        ),
        child: Row(
          children: [
            Icon(icon,
                size: 16,
                color: highlight
                    ? AppTheme.primaryBlue
                    : AppTheme.textSecondary),
            const SizedBox(width: 8),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    style: const TextStyle(
                      color: AppTheme.textSecondary,
                      fontSize: 10,
                      fontWeight: FontWeight.w500,
                      letterSpacing: 0.2,
                    ),
                  ),
                  Text(
                    value,
                    style: TextStyle(
                      color: highlight
                          ? AppTheme.primaryBlue
                          : AppTheme.textPrimary,
                      fontSize: 14,
                      fontWeight: FontWeight.w700,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Build ─────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      backgroundColor: AppTheme.backgroundWhite,
      appBar: AppBar(
        backgroundColor: AppTheme.primaryBlue,
        foregroundColor: Colors.white,
        elevation: 0,
        leading: const BackButton(color: Colors.white),
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Historial',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              'Cotizaciones generadas',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded,
                color: Colors.white),
            tooltip: 'Recargar',
            onPressed: _recargar,
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: FutureBuilder<List<dynamic>>(
        future: _futuro,
        builder: (context, snapshot) {
          // ── Cargando ────────────────────────────────────────────────────
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  CircularProgressIndicator(
                    valueColor: AlwaysStoppedAnimation<Color>(
                        AppTheme.primaryBlue),
                    strokeWidth: 2.5,
                  ),
                  SizedBox(height: 16),
                  Text(
                    'Cargando cotizaciones…',
                    style: TextStyle(
                        color: AppTheme.textSecondary, fontSize: 13),
                  ),
                ],
              ),
            );
          }

          // ── Error ────────────────────────────────────────────────────────
          if (snapshot.hasError) {
            final msg = snapshot.error
                .toString()
                .replaceFirst('Exception: ', '');
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.cloud_off_rounded,
                        size: 56,
                        color: AppTheme.errorColor.withValues(alpha: 0.7)),
                    const SizedBox(height: 16),
                    Text(
                      msg,
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                          color: AppTheme.textSecondary, fontSize: 14),
                    ),
                    const SizedBox(height: 20),
                    OutlinedButton.icon(
                      onPressed: _recargar,
                      icon: const Icon(Icons.refresh_rounded, size: 18),
                      label: const Text('Reintentar'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppTheme.primaryBlue,
                        side: BorderSide(
                            color:
                                AppTheme.primaryBlue.withValues(alpha: 0.5)),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10)),
                      ),
                    ),
                  ],
                ),
              ),
            );
          }

          final lista = snapshot.data ?? [];

          // ── Lista vacía ──────────────────────────────────────────────────
          if (lista.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.receipt_long_rounded,
                      size: 64,
                      color: AppTheme.textSecondary.withValues(alpha: 0.3)),
                  const SizedBox(height: 16),
                  Text(
                    'Aún no tienes cotizaciones',
                    style: theme.textTheme.titleMedium?.copyWith(
                        color: AppTheme.textSecondary),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    'Las cotizaciones generadas aparecerán aquí.',
                    style: theme.textTheme.bodySmall
                        ?.copyWith(color: AppTheme.textSecondary),
                  ),
                ],
              ),
            );
          }

          // ── Lista con datos ──────────────────────────────────────────────
          return ListView.builder(
            padding: const EdgeInsets.fromLTRB(16, 20, 16, 24),
            itemCount: lista.length,
            itemBuilder: (ctx, i) {
              final item = lista[i] as Map<String, dynamic>;
              return _buildCard(item);
            },
          );
        },
      ),
    );
  }
}
