import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/core/constants/app_constants.dart';

// ─── Modelos ──────────────────────────────────────────────────────────────────
class DashboardData {
  final int totalPedidos;
  final int pedidosActivos;
  final double ventasMes;
  final int proformasPendientes;
  final int pedidosEntregados;
  final int pedidosEnFabrica;

  const DashboardData({
    required this.totalPedidos,
    required this.pedidosActivos,
    required this.ventasMes,
    required this.proformasPendientes,
    required this.pedidosEntregados,
    required this.pedidosEnFabrica,
  });

  factory DashboardData.fromJson(Map<String, dynamic> j) => DashboardData(
        totalPedidos: j['totalPedidos'] as int? ?? 0,
        pedidosActivos: j['pedidosActivos'] as int? ?? 0,
        ventasMes: (j['ventasMes'] as num?)?.toDouble() ?? 0,
        proformasPendientes: j['proformasPendientes'] as int? ?? 0,
        pedidosEntregados: j['pedidosEntregados'] as int? ?? 0,
        pedidosEnFabrica: j['pedidosEnFabrica'] as int? ?? 0,
      );
}

class MejorCliente {
  final String nombre;
  final int totalPedidos;
  final double totalComprado;

  const MejorCliente(
      {required this.nombre,
      required this.totalPedidos,
      required this.totalComprado});

  factory MejorCliente.fromJson(Map<String, dynamic> j) => MejorCliente(
        nombre: j['nombre'] as String? ?? '',
        totalPedidos: j['totalPedidos'] as int? ?? 0,
        totalComprado: (j['totalComprado'] as num?)?.toDouble() ?? 0,
      );
}

class RendimientoVendedor {
  final String nombre;
  final int proformasCreadas;
  final int proformasAceptadas;
  final double tasaConversion;

  const RendimientoVendedor({
    required this.nombre,
    required this.proformasCreadas,
    required this.proformasAceptadas,
    required this.tasaConversion,
  });

  factory RendimientoVendedor.fromJson(Map<String, dynamic> j) =>
      RendimientoVendedor(
        nombre: j['nombre'] as String? ?? '',
        proformasCreadas: j['proformasCreadas'] as int? ?? 0,
        proformasAceptadas: j['proformasAceptadas'] as int? ?? 0,
        tasaConversion: (j['tasaConversion'] as num?)?.toDouble() ?? 0,
      );
}

// ─── Pantalla de Reportes ─────────────────────────────────────────────────────
class ReportesScreen extends StatefulWidget {
  const ReportesScreen({super.key});

  @override
  State<ReportesScreen> createState() => _ReportesScreenState();
}

class _ReportesScreenState extends State<ReportesScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;
  final _fmt = NumberFormat('#,##0.00', 'es_BO');

  DashboardData? _dashboard;
  List<MejorCliente> _clientes = [];
  List<RendimientoVendedor> _vendedores = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _cargar();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<Map<String, String>> _headers() async {
    final token = await AuthService().getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  Future<void> _cargar() async {
    setState(() { _cargando = true; _error = null; });
    try {
      final h = await _headers();
      final results = await Future.wait([
        http.get(Uri.parse('${AppConstants.baseUrl}/reportes/dashboard'), headers: h),
        http.get(Uri.parse('${AppConstants.baseUrl}/reportes/mejores-clientes'), headers: h),
        http.get(Uri.parse('${AppConstants.baseUrl}/reportes/rendimiento-vendedores'), headers: h),
      ]);
      if (results.every((r) => r.statusCode == 200)) {
        setState(() {
          _dashboard = DashboardData.fromJson(jsonDecode(results[0].body));
          _clientes = (jsonDecode(results[1].body) as List)
              .map((e) => MejorCliente.fromJson(e))
              .toList();
          _vendedores = (jsonDecode(results[2].body) as List)
              .map((e) => RendimientoVendedor.fromJson(e))
              .toList();
          _cargando = false;
        });
      } else {
        throw Exception('Error al cargar reportes');
      }
    } catch (e) {
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _cargando = false;
      });
    }
  }

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
            Text('Reportes y Métricas',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: Colors.white, fontWeight: FontWeight.w700)),
            Text('Vista gerencial',
                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                      color: Colors.white70, letterSpacing: 0.5)),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded, color: Colors.white),
            onPressed: _cargando ? null : _cargar,
          ),
          const SizedBox(width: 4),
        ],
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: Colors.white,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white70,
          tabs: const [
            Tab(text: 'Dashboard'),
            Tab(text: 'Clientes'),
            Tab(text: 'Vendedores'),
          ],
        ),
      ),
      body: _cargando
          ? const Center(
              child: CircularProgressIndicator(color: AppTheme.primaryBlue))
          : _error != null
              ? _buildError()
              : TabBarView(
                  controller: _tabController,
                  children: [
                    _buildDashboard(),
                    _buildClientes(),
                    _buildVendedores(),
                  ],
                ),
    );
  }

  Widget _buildError() => Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.cloud_off_rounded,
                size: 64, color: AppTheme.errorColor.withValues(alpha: 0.7)),
            const SizedBox(height: 16),
            Text(_error!,
                style: const TextStyle(
                    color: AppTheme.textSecondary, fontSize: 13),
                textAlign: TextAlign.center),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              style: ElevatedButton.styleFrom(
                backgroundColor: AppTheme.primaryBlue,
                foregroundColor: Colors.white,
                minimumSize: const Size(160, 46),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12)),
              ),
              onPressed: _cargar,
              icon: const Icon(Icons.refresh_rounded, size: 18),
              label: const Text('Reintentar'),
            ),
          ],
        ),
      );

  // ── Tab 1: Dashboard KPIs ─────────────────────────────────────────────────
  Widget _buildDashboard() {
    final d = _dashboard!;
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Resumen del mes',
              style: TextStyle(
                  color: AppTheme.textPrimary,
                  fontSize: 16,
                  fontWeight: FontWeight.w700)),
          const SizedBox(height: 4),
          Text(DateFormat('MMMM yyyy', 'es').format(DateTime.now()),
              style: const TextStyle(
                  color: AppTheme.textSecondary, fontSize: 13)),
          const SizedBox(height: 20),
          // KPI grande – ventas
          _kpiBig(
            titulo: 'Ventas del mes',
            valor: 'Bs ${_fmt.format(d.ventasMes)}',
            icono: Icons.attach_money_rounded,
            color: const Color(0xFF27AE60),
          ),
          const SizedBox(height: 12),
          // KPIs en grid 2x2
          GridView.count(
            crossAxisCount: 2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            mainAxisSpacing: 12,
            crossAxisSpacing: 12,
            childAspectRatio: 1.6,
            children: [
              _kpiCard('Total pedidos', '${d.totalPedidos}',
                  Icons.shopping_bag_rounded, AppTheme.primaryBlue),
              _kpiCard('Pedidos activos', '${d.pedidosActivos}',
                  Icons.pending_actions_rounded, const Color(0xFFFFC107)),
              _kpiCard('En fábrica', '${d.pedidosEnFabrica}',
                  Icons.precision_manufacturing_rounded,
                  const Color(0xFF2196F3)),
              _kpiCard('Entregados', '${d.pedidosEntregados}',
                  Icons.check_circle_rounded, const Color(0xFF4CAF50)),
            ],
          ),
          const SizedBox(height: 12),
          _kpiCard(
            'Proformas pendientes',
            '${d.proformasPendientes}',
            Icons.description_rounded,
            const Color(0xFFE53935),
            fullWidth: true,
          ),
        ],
      ),
    );
  }

  Widget _kpiBig(
      {required String titulo,
      required String valor,
      required IconData icono,
      required Color color}) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.06),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: color.withValues(alpha: 0.25), width: 1),
      ),
      child: Row(
        children: [
          Container(
            width: 52,
            height: 52,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(14),
            ),
            child: Center(child: Icon(icono, color: color, size: 26)),
          ),
          const SizedBox(width: 16),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(titulo,
                  style: TextStyle(
                      color: AppTheme.textSecondary,
                      fontSize: 13,
                      fontWeight: FontWeight.w500)),
              const SizedBox(height: 4),
              Text(valor,
                  style: TextStyle(
                      color: color,
                      fontSize: 24,
                      fontWeight: FontWeight.w800)),
            ],
          ),
        ],
      ),
    );
  }

  Widget _kpiCard(String titulo, String valor, IconData icono, Color color,
      {bool fullWidth = false}) {
    final card = Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppTheme.cardColor,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
            color: AppTheme.accentGrey.withValues(alpha: 0.15), width: 1),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withValues(alpha: 0.03),
              blurRadius: 8,
              offset: const Offset(0, 2))
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 38,
            height: 38,
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Center(child: Icon(icono, color: color, size: 20)),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(titulo,
                    style: const TextStyle(
                        color: AppTheme.textSecondary,
                        fontSize: 11,
                        fontWeight: FontWeight.w500),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis),
                const SizedBox(height: 3),
                Text(valor,
                    style: TextStyle(
                        color: color,
                        fontSize: 20,
                        fontWeight: FontWeight.w800)),
              ],
            ),
          ),
        ],
      ),
    );
    return fullWidth ? card : card;
  }

  // ── Tab 2: Mejores Clientes ───────────────────────────────────────────────
  Widget _buildClientes() {
    if (_clientes.isEmpty) {
      return Center(
          child: Text('Sin datos de clientes',
              style: TextStyle(
                  color: AppTheme.textSecondary.withValues(alpha: 0.6),
                  fontSize: 14)));
    }
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
      itemCount: _clientes.length,
      itemBuilder: (_, i) {
        final c = _clientes[i];
        final esTop = i < 3;
        final medallas = ['🥇', '🥈', '🥉'];
        return Container(
          margin: const EdgeInsets.only(bottom: 12),
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: AppTheme.cardColor,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(
                color: esTop
                    ? AppTheme.primaryBlue.withValues(alpha: 0.25)
                    : AppTheme.accentGrey.withValues(alpha: 0.15)),
            boxShadow: [
              BoxShadow(
                  color: Colors.black.withValues(alpha: 0.03),
                  blurRadius: 8,
                  offset: const Offset(0, 2))
            ],
          ),
          child: Row(
            children: [
              SizedBox(
                width: 32,
                child: Text(
                  esTop ? medallas[i] : '${i + 1}',
                  style: TextStyle(
                      fontSize: esTop ? 20 : 14,
                      fontWeight: FontWeight.w700,
                      color: AppTheme.textSecondary),
                  textAlign: TextAlign.center,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(c.nombre,
                        style: const TextStyle(
                            color: AppTheme.textPrimary,
                            fontWeight: FontWeight.w600,
                            fontSize: 14)),
                    const SizedBox(height: 3),
                    Text('${c.totalPedidos} pedido${c.totalPedidos == 1 ? '' : 's'}',
                        style: const TextStyle(
                            color: AppTheme.textSecondary, fontSize: 12)),
                  ],
                ),
              ),
              Text(
                'Bs ${_fmt.format(c.totalComprado)}',
                style: const TextStyle(
                    color: AppTheme.primaryBlue,
                    fontWeight: FontWeight.w700,
                    fontSize: 14),
              ),
            ],
          ),
        );
      },
    );
  }

  // ── Tab 3: Rendimiento Vendedores ────────────────────────────────────────
  Widget _buildVendedores() {
    if (_vendedores.isEmpty) {
      return Center(
          child: Text('Sin datos de vendedores',
              style: TextStyle(
                  color: AppTheme.textSecondary.withValues(alpha: 0.6),
                  fontSize: 14)));
    }
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
      itemCount: _vendedores.length,
      itemBuilder: (_, i) {
        final v = _vendedores[i];
        final pct = v.tasaConversion.clamp(0.0, 100.0);
        return Container(
          margin: const EdgeInsets.only(bottom: 14),
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: AppTheme.cardColor,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(
                color: AppTheme.accentGrey.withValues(alpha: 0.15)),
            boxShadow: [
              BoxShadow(
                  color: Colors.black.withValues(alpha: 0.03),
                  blurRadius: 8,
                  offset: const Offset(0, 2))
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      color: AppTheme.primaryBlue.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Center(
                      child: Text(
                        v.nombre.isNotEmpty
                            ? v.nombre[0].toUpperCase()
                            : '?',
                        style: const TextStyle(
                            color: AppTheme.primaryBlue,
                            fontWeight: FontWeight.w800,
                            fontSize: 16),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(v.nombre,
                        style: const TextStyle(
                            color: AppTheme.textPrimary,
                            fontWeight: FontWeight.w700,
                            fontSize: 15)),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: AppTheme.primaryBlue.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text(
                      '${pct.toStringAsFixed(0)}% conv.',
                      style: const TextStyle(
                          color: AppTheme.primaryBlue,
                          fontSize: 12,
                          fontWeight: FontWeight.w700),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 14),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  _statChip(
                    label: 'Proformas',
                    valor: '${v.proformasCreadas}',
                    color: AppTheme.textSecondary,
                  ),
                  _statChip(
                    label: 'Aceptadas',
                    valor: '${v.proformasAceptadas}',
                    color: const Color(0xFF27AE60),
                  ),
                ],
              ),
              const SizedBox(height: 10),
              // Barra de conversión
              ClipRRect(
                borderRadius: BorderRadius.circular(4),
                child: LinearProgressIndicator(
                  value: pct / 100,
                  minHeight: 6,
                  backgroundColor:
                      AppTheme.accentGrey.withValues(alpha: 0.15),
                  valueColor: const AlwaysStoppedAnimation<Color>(
                      AppTheme.primaryBlue),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _statChip(
      {required String label,
      required String valor,
      required Color color}) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label,
            style: const TextStyle(
                color: AppTheme.textSecondary,
                fontSize: 11,
                fontWeight: FontWeight.w500)),
        const SizedBox(height: 2),
        Text(valor,
            style: TextStyle(
                color: color, fontSize: 18, fontWeight: FontWeight.w700)),
      ],
    );
  }
}
