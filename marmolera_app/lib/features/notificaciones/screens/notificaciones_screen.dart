import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/core/constants/api_constants.dart';

// ─── Modelo ───────────────────────────────────────────────────────────────────
class Notificacion {
  final int id;
  final String mensaje;
  final int tipo;
  final bool leida;
  final DateTime createdAt;

  const Notificacion({
    required this.id,
    required this.mensaje,
    required this.tipo,
    required this.leida,
    required this.createdAt,
  });

  factory Notificacion.fromJson(Map<String, dynamic> j) => Notificacion(
        id: j['id'] as int,
        mensaje: j['mensaje'] as String? ?? '',
        tipo: j['tipo'] as int? ?? 0,
        leida: j['leida'] as bool? ?? false,
        createdAt: DateTime.parse(j['createdAt'] as String),
      );

  IconData get icono {
    switch (tipo) {
      case 0: return Icons.assignment_late_rounded;   // PedidoSinAvance
      case 1: return Icons.schedule_rounded;           // PedidoProximoEntrega
      case 2: return Icons.image_rounded;              // NuevaOrden
      case 3: return Icons.check_circle_rounded;       // PedidoTerminado
      case 4: return Icons.local_shipping_rounded;     // PedidoListoEntrega
      case 5: return Icons.warning_amber_rounded;      // ProblemaReportado
      case 6: return Icons.person_pin_circle_rounded;  // ContactarCliente
      default: return Icons.notifications_rounded;
    }
  }

  Color get colorIcono {
    switch (tipo) {
      case 0: return const Color(0xFFE53935);
      case 1: return const Color(0xFFFFC107);
      case 2: return AppTheme.primaryBlue;
      case 3: return const Color(0xFF4CAF50);
      case 4: return const Color(0xFF4CAF50);
      case 5: return const Color(0xFFE53935);
      case 6: return AppTheme.primaryBlue;
      default: return AppTheme.accentGrey;
    }
  }
}

// ─── Pantalla de Notificaciones ───────────────────────────────────────────────
class NotificacionesScreen extends StatefulWidget {
  const NotificacionesScreen({super.key});

  @override
  State<NotificacionesScreen> createState() => _NotificacionesScreenState();
}

class _NotificacionesScreenState extends State<NotificacionesScreen> {
  List<Notificacion> _notificaciones = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargar();
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
      final res = await http.get(
        Uri.parse('${ApiConstants.baseUrl}/notificaciones'),
        headers: await _headers(),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        setState(() {
          _notificaciones = list.map((e) => Notificacion.fromJson(e)).toList();
          _cargando = false;
        });
      } else {
        throw Exception('Error ${res.statusCode}');
      }
    } catch (e) {
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _cargando = false;
      });
    }
  }

  Future<void> _marcarLeida(int id) async {
    try {
      await http.put(
        Uri.parse('${ApiConstants.baseUrl}/notificaciones/$id/leer'),
        headers: await _headers(),
      );
      setState(() {
        final idx = _notificaciones.indexWhere((n) => n.id == id);
        if (idx != -1) {
          _notificaciones[idx] = Notificacion(
            id: _notificaciones[idx].id,
            mensaje: _notificaciones[idx].mensaje,
            tipo: _notificaciones[idx].tipo,
            leida: true,
            createdAt: _notificaciones[idx].createdAt,
          );
        }
      });
    } catch (_) {}
  }

  Future<void> _marcarTodasLeidas() async {
    try {
      await http.put(
        Uri.parse('${ApiConstants.baseUrl}/notificaciones/leer-todas'),
        headers: await _headers(),
      );
      await _cargar();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(e.toString().replaceFirst('Exception: ', '')),
          backgroundColor: AppTheme.errorColor,
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.all(16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        ));
      }
    }
  }

  int get _noLeidas => _notificaciones.where((n) => !n.leida).length;

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
            Text('Notificaciones',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: Colors.white, fontWeight: FontWeight.w700)),
            if (_noLeidas > 0)
              Text('$_noLeidas sin leer',
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(
                        color: Colors.white70, letterSpacing: 0.5)),
          ],
        ),
        actions: [
          if (_noLeidas > 0)
            TextButton(
              onPressed: _marcarTodasLeidas,
              child: const Text('Marcar todo',
                  style: TextStyle(color: Colors.white70, fontSize: 13)),
            ),
          IconButton(
            icon: const Icon(Icons.refresh_rounded, color: Colors.white),
            onPressed: _cargando ? null : _cargar,
          ),
          const SizedBox(width: 4),
        ],
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_cargando) {
      return const Center(
          child: CircularProgressIndicator(color: AppTheme.primaryBlue));
    }
    if (_error != null) {
      return Center(
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
    }
    if (_notificaciones.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.notifications_none_rounded,
                size: 72,
                color: AppTheme.accentGrey.withValues(alpha: 0.3)),
            const SizedBox(height: 20),
            const Text('Sin notificaciones',
                style: TextStyle(
                    color: AppTheme.textPrimary,
                    fontSize: 18,
                    fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text('Todo está al día por el momento.',
                style: TextStyle(
                    color: AppTheme.textSecondary.withValues(alpha: 0.7),
                    fontSize: 13)),
          ],
        ),
      );
    }
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 32),
      itemCount: _notificaciones.length,
      itemBuilder: (_, i) => _buildItem(_notificaciones[i]),
    );
  }

  Widget _buildItem(Notificacion n) {
    return GestureDetector(
      onTap: () { if (!n.leida) _marcarLeida(n.id); },
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        decoration: BoxDecoration(
          color: n.leida
              ? AppTheme.cardColor
              : AppTheme.primaryBlue.withValues(alpha: 0.04),
          borderRadius: BorderRadius.circular(14),
          border: Border.all(
            color: n.leida
                ? AppTheme.accentGrey.withValues(alpha: 0.15)
                : AppTheme.primaryBlue.withValues(alpha: 0.2),
            width: 1,
          ),
          boxShadow: [
            BoxShadow(
                color: Colors.black.withValues(alpha: 0.03),
                blurRadius: 8,
                offset: const Offset(0, 2))
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 42,
                height: 42,
                decoration: BoxDecoration(
                  color: n.colorIcono.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Center(
                  child: Icon(n.icono, color: n.colorIcono, size: 20),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      n.mensaje,
                      style: TextStyle(
                        color: AppTheme.textPrimary,
                        fontSize: 13,
                        fontWeight: n.leida ? FontWeight.w400 : FontWeight.w600,
                        height: 1.4,
                      ),
                    ),
                    const SizedBox(height: 5),
                    Text(
                      DateFormat('dd/MM/yyyy HH:mm').format(n.createdAt.toLocal()),
                      style: TextStyle(
                          color: AppTheme.textSecondary.withValues(alpha: 0.7),
                          fontSize: 11),
                    ),
                  ],
                ),
              ),
              if (!n.leida)
                Container(
                  width: 8,
                  height: 8,
                  margin: const EdgeInsets.only(top: 4, left: 8),
                  decoration: const BoxDecoration(
                    color: AppTheme.primaryBlue,
                    shape: BoxShape.circle,
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}
