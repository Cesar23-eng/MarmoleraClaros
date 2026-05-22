import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/auth/services/auth_service.dart';
import 'package:marmolera_app/core/constants/app_constants.dart';

// ─── Modelo ───────────────────────────────────────────────────────────────────
class EventoCalendario {
  final int id;
  final String titulo;
  final DateTime fecha;
  final int tipo; // 0=TomaMedida 1=EntregaEstimada 2=EntregaReal 3=Problema
  final String? notas;
  final int? pedidoId;

  const EventoCalendario({
    required this.id,
    required this.titulo,
    required this.fecha,
    required this.tipo,
    this.notas,
    this.pedidoId,
  });

  factory EventoCalendario.fromJson(Map<String, dynamic> j) => EventoCalendario(
        id: j['id'] as int,
        titulo: j['titulo'] as String? ?? '',
        fecha: DateTime.parse(j['fecha'] as String),
        tipo: j['tipo'] as int? ?? 0,
        notas: j['notas'] as String?,
        pedidoId: j['pedidoId'] as int?,
      );

  Color get color {
    switch (tipo) {
      case 0: return const Color(0xFF2196F3);  // 🔵 Toma de medida
      case 1: return const Color(0xFFFFC107);  // 🟡 Entrega estimada
      case 2: return const Color(0xFF4CAF50);  // 🟢 Entrega real
      case 3: return const Color(0xFFE53935);  // 🔴 Problema
      default: return AppTheme.accentGrey;
    }
  }

  String get tipoLabel {
    switch (tipo) {
      case 0: return 'Toma de medida';
      case 1: return 'Entrega estimada';
      case 2: return 'Entrega real';
      case 3: return 'Problema';
      default: return 'Evento';
    }
  }
}

// ─── Pantalla de Calendario ───────────────────────────────────────────────────
class CalendarioScreen extends StatefulWidget {
  const CalendarioScreen({super.key});

  @override
  State<CalendarioScreen> createState() => _CalendarioScreenState();
}

class _CalendarioScreenState extends State<CalendarioScreen> {
  DateTime _mesActual = DateTime(DateTime.now().year, DateTime.now().month);
  DateTime? _diaSeleccionado;
  List<EventoCalendario> _eventos = [];
  bool _cargando = true;
  String? _error;

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
      final desde = DateTime(_mesActual.year, _mesActual.month, 1);
      final hasta = DateTime(_mesActual.year, _mesActual.month + 1, 0);
      final url = Uri.parse(
          '${AppConstants.baseUrl}/calendario?desde=${desde.toIso8601String()}&hasta=${hasta.toIso8601String()}');
      final res = await http.get(url, headers: await _headers());
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        setState(() {
          _eventos = list.map((e) => EventoCalendario.fromJson(e)).toList();
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

  Future<void> _crearEvento({
    required int tipo,
    required DateTime fecha,
    required String titulo,
    String? notas,
    int? pedidoId,
  }) async {
    try {
      final res = await http.post(
        Uri.parse('${AppConstants.baseUrl}/calendario'),
        headers: await _headers(),
        body: jsonEncode({
          'tipo': tipo,
          'fecha': fecha.toIso8601String(),
          'titulo': titulo,
          if (notas != null) 'notas': notas,
          if (pedidoId != null) 'pedidoId': pedidoId,
        }),
      );
      if (res.statusCode == 200 || res.statusCode == 201) {
        await _cargar();
      } else {
        throw Exception('Error ${res.statusCode}');
      }
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

  // ── Helpers calendario ────────────────────────────────────────────────────

  List<EventoCalendario> _eventosDelDia(DateTime dia) => _eventos
      .where((e) =>
          e.fecha.year == dia.year &&
          e.fecha.month == dia.month &&
          e.fecha.day == dia.day)
      .toList();

  List<DateTime?> _diasDelMes() {
    final primero = DateTime(_mesActual.year, _mesActual.month, 1);
    final ultimo = DateTime(_mesActual.year, _mesActual.month + 1, 0);
    final offsetInicio = (primero.weekday % 7); // Domingo=0
    final List<DateTime?> dias = List.filled(offsetInicio, null);
    for (int d = 1; d <= ultimo.day; d++) {
      dias.add(DateTime(_mesActual.year, _mesActual.month, d));
    }
    while (dias.length % 7 != 0) dias.add(null);
    return dias;
  }

  void _mostrarDialogoNuevoEvento(DateTime dia) {
    final controllerTitulo = TextEditingController();
    int tipoSeleccionado = 0;
    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx2, setLocal) => AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
          title: Text(
            'Nuevo evento – ${DateFormat('dd/MM/yyyy').format(dia)}',
            style: const TextStyle(
                color: AppTheme.textPrimary,
                fontWeight: FontWeight.w700,
                fontSize: 16),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: controllerTitulo,
                decoration: const InputDecoration(
                  labelText: 'Título',
                  prefixIcon: Icon(Icons.title_rounded),
                ),
              ),
              const SizedBox(height: 16),
              const Align(
                alignment: Alignment.centerLeft,
                child: Text('Tipo de evento',
                    style: TextStyle(
                        color: AppTheme.textSecondary,
                        fontSize: 12,
                        fontWeight: FontWeight.w600)),
              ),
              const SizedBox(height: 8),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  _chipTipo(0, '🔵 Toma de medida', tipoSeleccionado, (v) => setLocal(() => tipoSeleccionado = v)),
                  _chipTipo(1, '🟡 Entrega estimada', tipoSeleccionado, (v) => setLocal(() => tipoSeleccionado = v)),
                  _chipTipo(2, '🟢 Entrega real', tipoSeleccionado, (v) => setLocal(() => tipoSeleccionado = v)),
                  _chipTipo(3, '🔴 Problema', tipoSeleccionado, (v) => setLocal(() => tipoSeleccionado = v)),
                ],
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Cancelar'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: AppTheme.primaryBlue,
                foregroundColor: Colors.white,
                minimumSize: const Size(0, 40),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10)),
              ),
              onPressed: () {
                Navigator.pop(ctx);
                if (controllerTitulo.text.trim().isNotEmpty) {
                  _crearEvento(
                    tipo: tipoSeleccionado,
                    fecha: dia,
                    titulo: controllerTitulo.text.trim(),
                  );
                }
              },
              child: const Text('Guardar'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _chipTipo(int valor, String label, int seleccionado, void Function(int) onTap) {
    final selected = valor == seleccionado;
    return GestureDetector(
      onTap: () => onTap(valor),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
        decoration: BoxDecoration(
          color: selected
              ? AppTheme.primaryBlue.withValues(alpha: 0.12)
              : AppTheme.accentGrey.withValues(alpha: 0.08),
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: selected
                ? AppTheme.primaryBlue.withValues(alpha: 0.5)
                : AppTheme.accentGrey.withValues(alpha: 0.2),
          ),
        ),
        child: Text(label,
            style: TextStyle(
                fontSize: 12,
                fontWeight: selected ? FontWeight.w700 : FontWeight.w400,
                color: selected ? AppTheme.primaryBlue : AppTheme.textSecondary)),
      ),
    );
  }

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
            Text('Calendario',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: Colors.white, fontWeight: FontWeight.w700)),
            Text('Programación y entregas',
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
      ),
      floatingActionButton: FloatingActionButton(
        backgroundColor: AppTheme.primaryBlue,
        foregroundColor: Colors.white,
        tooltip: 'Nuevo evento',
        onPressed: () => _mostrarDialogoNuevoEvento(
            _diaSeleccionado ?? DateTime.now()),
        child: const Icon(Icons.add_rounded),
      ),
      body: _cargando
          ? const Center(
              child: CircularProgressIndicator(color: AppTheme.primaryBlue))
          : _error != null
              ? _buildError()
              : _buildCalendario(),
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
                    color: AppTheme.textSecondary, fontSize: 13)),
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

  Widget _buildCalendario() {
    final dias = _diasDelMes();
    final eventosDiaSeleccionado =
        _diaSeleccionado != null ? _eventosDelDia(_diaSeleccionado!) : <EventoCalendario>[];

    return Column(
      children: [
        // ── Navegación mes ─────────────────────────────────────────────────
        Container(
          color: AppTheme.primaryBlue.withValues(alpha: 0.04),
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              IconButton(
                icon: const Icon(Icons.chevron_left_rounded,
                    color: AppTheme.primaryBlue),
                onPressed: () {
                  setState(() {
                    _mesActual = DateTime(
                        _mesActual.year, _mesActual.month - 1);
                    _diaSeleccionado = null;
                  });
                  _cargar();
                },
              ),
              Text(
                DateFormat('MMMM yyyy', 'es').format(_mesActual),
                style: const TextStyle(
                    color: AppTheme.textPrimary,
                    fontSize: 16,
                    fontWeight: FontWeight.w700),
              ),
              IconButton(
                icon: const Icon(Icons.chevron_right_rounded,
                    color: AppTheme.primaryBlue),
                onPressed: () {
                  setState(() {
                    _mesActual = DateTime(
                        _mesActual.year, _mesActual.month + 1);
                    _diaSeleccionado = null;
                  });
                  _cargar();
                },
              ),
            ],
          ),
        ),

        // ── Cabecera días semana ────────────────────────────────────────────
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          child: Row(
            children: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']
                .map((d) => Expanded(
                      child: Center(
                        child: Text(d,
                            style: TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.w600,
                                color: AppTheme.textSecondary
                                    .withValues(alpha: 0.7))),
                      ),
                    ))
                .toList(),
          ),
        ),

        // ── Grid de días ───────────────────────────────────────────────────
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8),
          child: GridView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 7,
              childAspectRatio: 1.0,
            ),
            itemCount: dias.length,
            itemBuilder: (_, i) {
              final dia = dias[i];
              if (dia == null) return const SizedBox();
              final eventos = _eventosDelDia(dia);
              final esHoy = dia.year == DateTime.now().year &&
                  dia.month == DateTime.now().month &&
                  dia.day == DateTime.now().day;
              final esSeleccionado = _diaSeleccionado != null &&
                  dia.year == _diaSeleccionado!.year &&
                  dia.month == _diaSeleccionado!.month &&
                  dia.day == _diaSeleccionado!.day;
              return GestureDetector(
                onTap: () => setState(() => _diaSeleccionado = dia),
                child: Container(
                  margin: const EdgeInsets.all(2),
                  decoration: BoxDecoration(
                    color: esSeleccionado
                        ? AppTheme.primaryBlue
                        : esHoy
                            ? AppTheme.primaryBlue.withValues(alpha: 0.1)
                            : Colors.transparent,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        '${dia.day}',
                        style: TextStyle(
                          fontSize: 13,
                          fontWeight: esHoy || esSeleccionado
                              ? FontWeight.w700
                              : FontWeight.w400,
                          color: esSeleccionado
                              ? Colors.white
                              : AppTheme.textPrimary,
                        ),
                      ),
                      if (eventos.isNotEmpty)
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: eventos
                              .take(3)
                              .map((e) => Container(
                                    width: 5,
                                    height: 5,
                                    margin: const EdgeInsets.symmetric(
                                        horizontal: 1),
                                    decoration: BoxDecoration(
                                      color: esSeleccionado
                                          ? Colors.white
                                          : e.color,
                                      shape: BoxShape.circle,
                                    ),
                                  ))
                              .toList(),
                        ),
                    ],
                  ),
                ),
              );
            },
          ),
        ),

        const Divider(height: 1),

        // ── Lista eventos del día seleccionado ─────────────────────────────
        Expanded(
          child: _diaSeleccionado == null
              ? Center(
                  child: Text(
                    'Seleccioná un día para ver los eventos',
                    style: TextStyle(
                        color: AppTheme.textSecondary.withValues(alpha: 0.6),
                        fontSize: 13),
                  ),
                )
              : eventosDiaSeleccionado.isEmpty
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.event_available_rounded,
                              size: 48,
                              color: AppTheme.accentGrey
                                  .withValues(alpha: 0.3)),
                          const SizedBox(height: 12),
                          Text(
                            'Sin eventos el ${_diaSeleccionado!.day}/${_diaSeleccionado!.month}',
                            style: TextStyle(
                                color: AppTheme.textSecondary
                                    .withValues(alpha: 0.7),
                                fontSize: 13),
                          ),
                        ],
                      ),
                    )
                  : ListView.builder(
                      padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
                      itemCount: eventosDiaSeleccionado.length,
                      itemBuilder: (_, i) =>
                          _buildEventoItem(eventosDiaSeleccionado[i]),
                    ),
        ),

        // ── Leyenda ────────────────────────────────────────────────────────
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
          decoration: BoxDecoration(
            color: AppTheme.primaryBlue.withValues(alpha: 0.03),
            border: Border(
                top: BorderSide(
                    color: AppTheme.accentGrey.withValues(alpha: 0.15))),
          ),
          child: Wrap(
            spacing: 16,
            runSpacing: 6,
            children: [
              _leyenda(const Color(0xFF2196F3), 'Toma de medida'),
              _leyenda(const Color(0xFFFFC107), 'Entrega estimada'),
              _leyenda(const Color(0xFF4CAF50), 'Entrega real'),
              _leyenda(const Color(0xFFE53935), 'Problema'),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildEventoItem(EventoCalendario evento) => Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: AppTheme.cardColor,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
              color: evento.color.withValues(alpha: 0.3), width: 1.2),
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
              width: 10,
              height: 10,
              decoration:
                  BoxDecoration(color: evento.color, shape: BoxShape.circle),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(evento.titulo,
                      style: const TextStyle(
                          color: AppTheme.textPrimary,
                          fontWeight: FontWeight.w600,
                          fontSize: 14)),
                  const SizedBox(height: 2),
                  Text(evento.tipoLabel,
                      style: const TextStyle(
                          color: AppTheme.textSecondary, fontSize: 12)),
                ],
              ),
            ),
            if (evento.pedidoId != null)
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                decoration: BoxDecoration(
                  color: AppTheme.primaryBlue.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text('PED-${evento.pedidoId}',
                    style: const TextStyle(
                        color: AppTheme.primaryBlue,
                        fontSize: 11,
                        fontWeight: FontWeight.w600)),
              ),
          ],
        ),
      );

  Widget _leyenda(Color color, String label) => Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
              width: 8,
              height: 8,
              decoration:
                  BoxDecoration(color: color, shape: BoxShape.circle)),
          const SizedBox(width: 5),
          Text(label,
              style: const TextStyle(
                  color: AppTheme.textSecondary, fontSize: 11)),
        ],
      );
}
