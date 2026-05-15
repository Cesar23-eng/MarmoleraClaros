import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/core/routes/app_routes.dart';
import 'package:marmolera_app/features/auth/screens/login_screen.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/features/fabrica/models/pedido_produccion.dart';
import 'package:marmolera_app/features/ventas/services/cotizaciones_service.dart';

enum TipoLista { Pendiente, EnProduccion, Terminado }

class FabricaDashboard extends StatefulWidget {
  const FabricaDashboard({super.key});

  @override
  State<FabricaDashboard> createState() => _FabricaDashboardState();
}

class _FabricaDashboardState extends State<FabricaDashboard> {
  final CotizacionesService _service = CotizacionesService();

  List<PedidoProduccion> _pedidosPendientes = [];
  List<PedidoProduccion> _pedidosEnProduccion = [];
  List<PedidoProduccion> _pedidosTerminados = [];

  bool _cargando = true;
  String? _error;
  int? _procesandoId;

  @override
  void initState() {
    super.initState();
    _cargarPedidos();
  }

  Future<void> _cargarPedidos() async {
    setState(() { _cargando = true; _error = null; });
    try {
      final results = await Future.wait([
        _service.obtenerPedidosProduccion(),
        _service.obtenerEnProduccion(),
        _service.obtenerTerminados(),
      ]);
      if (mounted) {
        setState(() {
          _pedidosPendientes   = results[0];
          _pedidosEnProduccion = results[1];
          _pedidosTerminados   = results[2];
          _cargando = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString().replaceFirst('Exception: ', '');
          _cargando = false;
        });
      }
    }
  }

  Future<void> _iniciarFabricacion(PedidoProduccion pedido) async {
    setState(() => _procesandoId = pedido.id);
    try {
      await _service.iniciarFabricacion(pedido.id);
      await _cargarPedidos();
      if (mounted) _mostrarExito('COT-${pedido.id} → En Producción');
    } catch (e) {
      if (mounted) _mostrarError(e.toString());
    } finally {
      if (mounted) setState(() => _procesandoId = null);
    }
  }

  Future<void> _finalizarFabricacion(PedidoProduccion pedido) async {
    setState(() => _procesandoId = pedido.id);
    try {
      await _service.finalizarProduccion(pedido.id);
      await _cargarPedidos();
      if (mounted) _mostrarExito('COT-${pedido.id} → Finalizado');
    } catch (e) {
      if (mounted) _mostrarError(e.toString());
    } finally {
      if (mounted) setState(() => _procesandoId = null);
    }
  }

  void _mostrarExito(String mensaje) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Row(children: [
        const Icon(Icons.check_circle_outline_rounded,
            color: Colors.white, size: 18),
        const SizedBox(width: 10),
        Expanded(child: Text(mensaje,
            style: const TextStyle(fontWeight: FontWeight.w600))),
      ]),
      backgroundColor: AppTheme.primaryBlue,
      behavior: SnackBarBehavior.floating,
      margin: const EdgeInsets.all(16),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      duration: const Duration(seconds: 3),
    ));
  }

  void _mostrarError(String errorMsg) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Row(children: [
        const Icon(Icons.error_outline, color: Colors.white, size: 18),
        const SizedBox(width: 10),
        Expanded(child: Text(
            errorMsg.replaceFirst('Exception: ', ''),
            overflow: TextOverflow.ellipsis)),
      ]),
      backgroundColor: AppTheme.errorColor,
      behavior: SnackBarBehavior.floating,
      margin: const EdgeInsets.all(16),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
    ));
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 3,
      child: Scaffold(
        backgroundColor: AppTheme.backgroundWhite,
        appBar: _buildAppBar(context),
        body: _buildBody(),
      ),
    );
  }

  PreferredSizeWidget _buildAppBar(BuildContext context) {
    final theme = Theme.of(context);
    return AppBar(
      backgroundColor: AppTheme.primaryBlue,
      elevation: 0,
      titleSpacing: 16,
      title: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Tablero de Fábrica',
              style: theme.textTheme.titleMedium?.copyWith(
                  color: Colors.white, fontWeight: FontWeight.w700)),
          Text('Gestión de producción',
              style: theme.textTheme.labelSmall?.copyWith(
                  color: Colors.white70, letterSpacing: 0.5)),
        ],
      ),
      actions: [
        // ── Calendario ────────────────────────────────────────
        IconButton(
          icon: const Icon(Icons.calendar_month_rounded, color: Colors.white),
          tooltip: 'Calendario',
          onPressed: () => AppRoutes.push(context, AppRoutes.calendario),
        ),
        // ── Notificaciones ─────────────────────────────────
        IconButton(
          icon: const Icon(Icons.notifications_outlined, color: Colors.white),
          tooltip: 'Notificaciones',
          onPressed: () =>
              AppRoutes.push(context, AppRoutes.notificaciones),
        ),
        // ── Actualizar ────────────────────────────────────────
        IconButton(
          icon: const Icon(Icons.refresh_rounded, color: Colors.white),
          tooltip: 'Recargar',
          onPressed: _cargando ? null : _cargarPedidos,
        ),
        // ── Cerrar sesión ─────────────────────────────────
        IconButton(
          icon: const Icon(Icons.logout_rounded, color: Colors.white70),
          tooltip: 'Cerrar sesión',
          onPressed: () async {
            await AuthService().logout();
            if (context.mounted) {
              await Navigator.pushReplacementNamed(context, AppRoutes.login);
            }
          },
        ),
        const SizedBox(width: 4),
      ],
      bottom: const TabBar(
        indicatorColor: Colors.white,
        labelColor: Colors.white,
        unselectedLabelColor: Colors.white70,
        tabs: [
          Tab(text: 'Pendientes'),
          Tab(text: 'En Producción'),
          Tab(text: 'Terminados'),
        ],
      ),
    );
  }

  Widget _buildBody() {
    if (_cargando) return _buildCargando();
    if (_error != null) return _buildError();
    return TabBarView(
      children: [
        _buildLista(_pedidosPendientes,   TipoLista.Pendiente,
            'Sin pedidos pendientes\nLas cotizaciones aprobadas aparecerán aquí.'),
        _buildLista(_pedidosEnProduccion, TipoLista.EnProduccion,
            'Sin pedidos en producción\nInicia la fabricación de pedidos pendientes.'),
        _buildLista(_pedidosTerminados,   TipoLista.Terminado,
            'Sin pedidos terminados\nLos pedidos finalizados aparecerán aquí.'),
      ],
    );
  }

  Widget _buildLista(
      List<PedidoProduccion> pedidos, TipoLista tipo, String msjVacio) {
    if (pedidos.isEmpty) return _buildVacio(msjVacio);
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
      itemCount: pedidos.length,
      itemBuilder: (ctx, i) => _buildTarjeta(pedidos[i], tipo),
    );
  }

  Widget _buildCargando() => const Center(
        child: CircularProgressIndicator(color: AppTheme.primaryBlue));

  Widget _buildError() => Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.cloud_off_rounded,
                  size: 64,
                  color: AppTheme.errorColor.withValues(alpha: 0.7)),
              const SizedBox(height: 16),
              const Text('No se pudo cargar el tablero',
                  style: TextStyle(
                      color: AppTheme.textPrimary,
                      fontSize: 18,
                      fontWeight: FontWeight.w700),
                  textAlign: TextAlign.center),
              const SizedBox(height: 8),
              Text(_error ?? 'Error desconocido.',
                  style: const TextStyle(
                      color: AppTheme.textSecondary, fontSize: 13),
                  textAlign: TextAlign.center),
              const SizedBox(height: 28),
              SizedBox(
                width: 180,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primaryBlue,
                    foregroundColor: Colors.white,
                    minimumSize: const Size(0, 46),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                  ),
                  onPressed: _cargarPedidos,
                  icon: const Icon(Icons.refresh_rounded, size: 18),
                  label: const Text('Reintentar'),
                ),
              ),
            ],
          ),
        ),
      );

  Widget _buildVacio(String msj) => Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.hourglass_empty_rounded,
                size: 72,
                color: AppTheme.accentGrey.withValues(alpha: 0.25)),
            const SizedBox(height: 20),
            Text(
              msj.split('\n').first,
              style: const TextStyle(
                  color: AppTheme.textPrimary,
                  fontSize: 18,
                  fontWeight: FontWeight.w700),
            ),
            const SizedBox(height: 8),
            Text(
              msj.split('\n').last,
              textAlign: TextAlign.center,
              style: TextStyle(
                  color: AppTheme.textSecondary.withValues(alpha: 0.7),
                  fontSize: 13,
                  height: 1.5),
            ),
            const SizedBox(height: 28),
            OutlinedButton.icon(
              onPressed: _cargarPedidos,
              icon: const Icon(Icons.refresh_rounded, size: 16),
              label: const Text('Actualizar'),
              style: OutlinedButton.styleFrom(
                foregroundColor: AppTheme.primaryBlue,
                side: BorderSide(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.5)),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12)),
              ),
            ),
          ],
        ),
      );

  Widget _buildTarjeta(PedidoProduccion pedido, TipoLista tipo) {
    final procesando   = _procesandoId == pedido.id;
    final isTerminado  = tipo == TipoLista.Terminado;
    final isPendiente  = tipo == TipoLista.Pendiente;
    final fechaAprobacion = pedido.fechaAprobacion != null
        ? DateFormat('dd/MM/yyyy HH:mm').format(pedido.fechaAprobacion!.toLocal())
        : 'Fecha desconocida';

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: AppTheme.cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
            color: AppTheme.accentGrey.withValues(alpha: 0.2), width: 1.0),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withValues(alpha: 0.04),
              blurRadius: 10,
              offset: const Offset(0, 4))
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ─ Encabezado ────────────────────────────────────────────
          Container(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 12),
            decoration: BoxDecoration(
              color: AppTheme.primaryBlue.withValues(alpha: 0.03),
              borderRadius:
                  const BorderRadius.vertical(top: Radius.circular(16)),
              border: Border(
                  bottom: BorderSide(
                      color: AppTheme.accentGrey.withValues(alpha: 0.1))),
            ),
            child: Row(
              children: [
                Container(
                  width: 42,
                  height: 42,
                  decoration: BoxDecoration(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Center(
                    child: Text(
                      pedido.inicialCliente,
                      style: const TextStyle(
                          color: AppTheme.primaryBlue,
                          fontSize: 18,
                          fontWeight: FontWeight.w800),
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        pedido.nombreCliente,
                        style: const TextStyle(
                            color: AppTheme.textPrimary,
                            fontSize: 15,
                            fontWeight: FontWeight.w700),
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 2),
                      Text(
                        'Aprobado: $fechaAprobacion',
                        style: TextStyle(
                            color:
                                AppTheme.textSecondary.withValues(alpha: 0.8),
                            fontSize: 11),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.1),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(
                        color: AppTheme.primaryBlue.withValues(alpha: 0.2)),
                  ),
                  child: Text(
                    'COT-${pedido.id}',
                    style: const TextStyle(
                        color: AppTheme.primaryBlue,
                        fontWeight: FontWeight.w700,
                        fontSize: 12,
                        letterSpacing: 0.5),
                  ),
                ),
              ],
            ),
          ),

          // ─ Mesones ──────────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(children: [
                  const Icon(Icons.countertops_rounded,
                      size: 14, color: AppTheme.primaryBlue),
                  const SizedBox(width: 6),
                  Text(
                    'Mesones (${pedido.detalles.length})',
                    style: TextStyle(
                        color: AppTheme.textSecondary.withValues(alpha: 0.9),
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                        letterSpacing: 0.3),
                  ),
                ]),
                const SizedBox(height: 10),
                ...pedido.detalles.asMap().entries.map((entry) {
                  final i = entry.key;
                  final detalle = entry.value;
                  return Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.symmetric(
                        horizontal: 12, vertical: 10),
                    decoration: BoxDecoration(
                      color: AppTheme.backgroundWhite,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(
                          color:
                              AppTheme.accentGrey.withValues(alpha: 0.2)),
                    ),
                    child: Row(children: [
                      Container(
                        width: 22,
                        height: 22,
                        decoration: BoxDecoration(
                          color: AppTheme.primaryBlue.withValues(alpha: 0.08),
                          shape: BoxShape.circle,
                        ),
                        child: Center(
                          child: Text('${i + 1}',
                              style: const TextStyle(
                                  color: AppTheme.primaryBlue,
                                  fontSize: 11,
                                  fontWeight: FontWeight.w700)),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(detalle.nombreMaterial,
                                style: const TextStyle(
                                    color: AppTheme.textPrimary,
                                    fontSize: 13,
                                    fontWeight: FontWeight.w600),
                                overflow: TextOverflow.ellipsis),
                            const SizedBox(height: 2),
                            Text(
                              '${detalle.geometriaLabel} · ${detalle.medidasLabel}',
                              style: TextStyle(
                                  color: AppTheme.textSecondary
                                      .withValues(alpha: 0.8),
                                  fontSize: 11),
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text('${detalle.areaTotal.toStringAsFixed(2)} m²',
                          style: const TextStyle(
                              color: AppTheme.primaryBlue,
                              fontSize: 12,
                              fontWeight: FontWeight.w600)),
                    ]),
                  );
                }),
                const SizedBox(height: 4),
                Row(children: [
                  const Icon(Icons.square_foot_outlined,
                      size: 13, color: AppTheme.primaryBlue),
                  const SizedBox(width: 4),
                  Text(
                    'Total: ${pedido.areaTotal.toStringAsFixed(2)} m²',
                    style: const TextStyle(
                        color: AppTheme.primaryBlue,
                        fontSize: 12,
                        fontWeight: FontWeight.w600),
                  ),
                ]),
              ],
            ),
          ),

          // ─ Divider y botón ─────────────────────────────────────
          if (!isTerminado)
            Padding(
              padding: const EdgeInsets.symmetric(
                  horizontal: 16, vertical: 14),
              child: Divider(
                  color: AppTheme.accentGrey.withValues(alpha: 0.2),
                  height: 1),
            )
          else
            const SizedBox(height: 16),

          if (!isTerminado)
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
              child: SizedBox(
                width: double.infinity,
                height: 50,
                child: ElevatedButton.icon(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primaryBlue,
                    foregroundColor: Colors.white,
                    elevation: procesando ? 0 : 2,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12)),
                  ),
                  icon: procesando
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(
                              strokeWidth: 2.5, color: Colors.white))
                      : Icon(
                          isPendiente
                              ? Icons.precision_manufacturing_rounded
                              : Icons.check_circle_outline_rounded,
                          size: 20),
                  label: Text(procesando
                      ? 'Procesando…'
                      : (isPendiente
                          ? 'Iniciar Fabricación'
                          : 'Finalizar Pedido')),
                  onPressed: procesando
                      ? null
                      : () {
                          if (isPendiente) {
                            _iniciarFabricacion(pedido);
                          } else {
                            _finalizarFabricacion(pedido);
                          }
                        },
                ),
              ),
            ),
        ],
      ),
    );
  }
}
