import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/core/constants/api_constants.dart';

// ─── Modelo liviano para tablets ─────────────────────────────────────────────
class PedidoSinOrden {
  final int id;
  final String numeroPedido;
  final int cantidadOrdenes;

  const PedidoSinOrden({
    required this.id,
    required this.numeroPedido,
    required this.cantidadOrdenes,
  });

  factory PedidoSinOrden.fromJson(Map<String, dynamic> j) => PedidoSinOrden(
        id: j['id'] as int,
        numeroPedido: j['numeroPedido'] as String? ?? 'PED-${j['id']}',
        cantidadOrdenes: j['cantidadOrdenes'] as int? ?? 0,
      );
}

// ─── Pantalla Tablet – solo subir órdenes ────────────────────────────────────
class TabletOrdenesScreen extends StatefulWidget {
  const TabletOrdenesScreen({super.key});

  @override
  State<TabletOrdenesScreen> createState() => _TabletOrdenesScreenState();
}

class _TabletOrdenesScreenState extends State<TabletOrdenesScreen> {
  final _picker = ImagePicker();
  List<PedidoSinOrden> _pedidos = [];
  bool _cargando = true;
  String? _error;
  int? _subiendoId;

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  // ── HTTP ──────────────────────────────────────────────────────────────────

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
        Uri.parse('${ApiConstants.baseUrl}/ordenes/pedidos-sin-orden'),
        headers: await _headers(),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        setState(() {
          _pedidos = list.map((e) => PedidoSinOrden.fromJson(e)).toList();
          _cargando = false;
        });
      } else {
        throw Exception('Error ${res.statusCode}');
      }
    } catch (e) {
      setState(() { _error = e.toString().replaceFirst('Exception: ', ''); _cargando = false; });
    }
  }

  Future<void> _subirOrden(PedidoSinOrden pedido) async {
    final picked = await _picker.pickImage(
      source: ImageSource.camera,
      imageQuality: 80,
      maxWidth: 1600,
    );
    if (picked == null) return;

    setState(() => _subiendoId = pedido.id);
    try {
      final bytes = await File(picked.path).readAsBytes();
      final base64 = base64Encode(bytes);
      final ext = picked.path.split('.').last.toLowerCase();
      final mime = ext == 'png' ? 'image/png' : 'image/jpeg';

      final res = await http.post(
        Uri.parse('${ApiConstants.baseUrl}/ordenes'),
        headers: await _headers(),
        body: jsonEncode({
          'pedidoId': pedido.id,
          'archivoBase64': base64,
          'mimeType': mime,
          'nombreArchivo': 'orden_${pedido.id}_${DateTime.now().millisecondsSinceEpoch}.$ext',
        }),
      );

      if (res.statusCode == 200 || res.statusCode == 201) {
        if (mounted) _ok('Orden subida correctamente');
        await _cargar();
      } else {
        throw Exception('Error al subir: ${res.statusCode}');
      }
    } catch (e) {
      if (mounted) _err(e.toString().replaceFirst('Exception: ', ''));
    } finally {
      if (mounted) setState(() => _subiendoId = null);
    }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  void _ok(String msg) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Row(children: [
          const Icon(Icons.check_circle_outline_rounded, color: Colors.white, size: 18),
          const SizedBox(width: 10),
          Expanded(child: Text(msg, style: const TextStyle(fontWeight: FontWeight.w600))),
        ]),
        backgroundColor: const Color(0xFF27AE60),
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ));

  void _err(String msg) => ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text(msg),
        backgroundColor: AppTheme.errorColor,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ));

  // ── Build ─────────────────────────────────────────────────────────────────

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
            Text('Subir Órdenes',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: Colors.white, fontWeight: FontWeight.w700)),
            Text('Escaneo rápido de órdenes físicas',
                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                      color: Colors.white70, letterSpacing: 0.5)),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded, color: Colors.white),
            onPressed: _cargando ? null : _cargar,
            tooltip: 'Actualizar',
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
        child: CircularProgressIndicator(color: AppTheme.primaryBlue),
      );
    }
    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.cloud_off_rounded,
                  size: 64, color: AppTheme.errorColor.withValues(alpha: 0.7)),
              const SizedBox(height: 16),
              const Text('No se pudo cargar',
                  style: TextStyle(
                      color: AppTheme.textPrimary,
                      fontSize: 18,
                      fontWeight: FontWeight.w700)),
              const SizedBox(height: 8),
              Text(_error!,
                  style: const TextStyle(
                      color: AppTheme.textSecondary, fontSize: 13),
                  textAlign: TextAlign.center),
              const SizedBox(height: 28),
              ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primaryBlue,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(180, 46),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                ),
                onPressed: _cargar,
                icon: const Icon(Icons.refresh_rounded, size: 18),
                label: const Text('Reintentar'),
              ),
            ],
          ),
        ),
      );
    }
    if (_pedidos.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.check_circle_outline_rounded,
                size: 72,
                color: const Color(0xFF27AE60).withValues(alpha: 0.4)),
            const SizedBox(height: 20),
            const Text('Todos los pedidos tienen órdenes',
                style: TextStyle(
                    color: AppTheme.textPrimary,
                    fontSize: 18,
                    fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            Text('No hay pedidos pendientes de orden escaneada.',
                style: TextStyle(
                    color: AppTheme.textSecondary.withValues(alpha: 0.7),
                    fontSize: 13)),
          ],
        ),
      );
    }
    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
      itemCount: _pedidos.length,
      itemBuilder: (_, i) => _buildTarjeta(_pedidos[i]),
    );
  }

  Widget _buildTarjeta(PedidoSinOrden pedido) {
    final subiendo = _subiendoId == pedido.id;
    return Container(
      margin: const EdgeInsets.only(bottom: 14),
      decoration: BoxDecoration(
        color: AppTheme.cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
            color: AppTheme.accentGrey.withValues(alpha: 0.2), width: 1),
        boxShadow: [
          BoxShadow(
              color: Colors.black.withValues(alpha: 0.04),
              blurRadius: 10,
              offset: const Offset(0, 4))
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            Container(
              width: 52,
              height: 52,
              decoration: BoxDecoration(
                color: AppTheme.primaryBlue.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(14),
              ),
              child: const Center(
                child: Icon(Icons.receipt_long_rounded,
                    color: AppTheme.primaryBlue, size: 26),
              ),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(pedido.numeroPedido,
                      style: const TextStyle(
                          color: AppTheme.textPrimary,
                          fontSize: 15,
                          fontWeight: FontWeight.w700)),
                  const SizedBox(height: 4),
                  Row(children: [
                    const Icon(Icons.image_outlined,
                        size: 13, color: AppTheme.textSecondary),
                    const SizedBox(width: 4),
                    Text(
                      '${pedido.cantidadOrdenes} orden${pedido.cantidadOrdenes == 1 ? '' : 'es'} subida${pedido.cantidadOrdenes == 1 ? '' : 's'}',
                      style: const TextStyle(
                          color: AppTheme.textSecondary, fontSize: 12),
                    ),
                  ]),
                ],
              ),
            ),
            const SizedBox(width: 12),
            SizedBox(
              height: 44,
              child: ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primaryBlue,
                  foregroundColor: Colors.white,
                  minimumSize: const Size(0, 44),
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12)),
                ),
                onPressed: subiendo ? null : () => _subirOrden(pedido),
                icon: subiendo
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                            strokeWidth: 2, color: Colors.white))
                    : const Icon(Icons.camera_alt_rounded, size: 18),
                label: Text(subiendo ? 'Subiendo…' : 'Escanear'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
