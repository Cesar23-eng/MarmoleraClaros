import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/ventas/services/clientes_service.dart';
import 'package:marmolera_app/features/ventas/services/cotizaciones_service.dart';

// ─── Enums ────────────────────────────────────────────────────────────────────

enum TipoPiedra {
  granito('Granito Chiquitano', 300.0),
  cuarzo('Cuarzo Blanco', 350.0),
  marmol('Mármol', 280.0);

  final String label;
  final double precioPorM2; // precio base para cálculo local
  const TipoPiedra(this.label, this.precioPorM2);
}

enum PlantillaGeometrica {
  rectangulo('Rectángulo', 0),
  formaL('Forma L', 1),
  formaU('Forma U', 2);

  final String label;
  final int valor;
  const PlantillaGeometrica(this.label, this.valor);
}

// ─── Modelo interno de un mesón agregado ─────────────────────────────────────

class _MesonItem {
  final TipoPiedra material;
  final PlantillaGeometrica geometria;
  final Map<String, double> medidas; // ladoA, ladoB, ladoC?, ancho?
  final double areaTotal;
  final double precioSubtotal;

  _MesonItem({
    required this.material,
    required this.geometria,
    required this.medidas,
    required this.areaTotal,
    required this.precioSubtotal,
  });

  /// Serializa las medidas a JSON string para `medidasJson`
  String get medidasJson => jsonEncode(medidas);
}

// ─── Pantalla ─────────────────────────────────────────────────────────────────

/// Pantalla 2 del flujo de ventas rediseñada como "Carrito de Cotización".
/// Permite agregar múltiples mesones antes de guardar la cotización completa.
class DetallesCotizacionScreen extends StatefulWidget {
  final ClienteDto cliente;
  const DetallesCotizacionScreen({super.key, required this.cliente});

  @override
  State<DetallesCotizacionScreen> createState() =>
      _DetallesCotizacionScreenState();
}

class _DetallesCotizacionScreenState
    extends State<DetallesCotizacionScreen> {
  // ── Formulario de mesón actual ─────────────────────────────────────────────
  final _formKey = GlobalKey<FormState>();
  TipoPiedra _tipoPiedra = TipoPiedra.granito;
  PlantillaGeometrica _plantilla = PlantillaGeometrica.rectangulo;
  final _ctrlA     = TextEditingController();
  final _ctrlB     = TextEditingController();
  final _ctrlC     = TextEditingController();
  final _ctrlAncho = TextEditingController();

  // ── Comentarios de la cotización completa ─────────────────────────────────
  final _ctrlComentarios = TextEditingController();

  // ── Estado del carrito ─────────────────────────────────────────────────────
  final List<_MesonItem> _mesonesAgregados = [];
  double get _precioTotal =>
      _mesonesAgregados.fold(0, (sum, m) => sum + m.precioSubtotal);

  // ── Estado API ─────────────────────────────────────────────────────────────
  bool _guardando = false;

  final CotizacionesService _service = CotizacionesService();

  @override
  void dispose() {
    _ctrlA.dispose();
    _ctrlB.dispose();
    _ctrlC.dispose();
    _ctrlAncho.dispose();
    _ctrlComentarios.dispose();
    super.dispose();
  }

  // ── Limpiar formulario de mesón ────────────────────────────────────────────
  void _limpiarFormulario() {
    _formKey.currentState?.reset();
    setState(() {
      _tipoPiedra = TipoPiedra.granito;
      _plantilla = PlantillaGeometrica.rectangulo;
    });
    _ctrlA.clear();
    _ctrlB.clear();
    _ctrlC.clear();
    _ctrlAncho.clear();
  }

  // ── Calcular área según plantilla ─────────────────────────────────────────
  double _calcularArea() {
    final a = double.tryParse(_ctrlA.text) ?? 0;
    final b = double.tryParse(_ctrlB.text) ?? 0;
    final c = double.tryParse(_ctrlC.text) ?? 0;
    final ancho = double.tryParse(_ctrlAncho.text) ?? 0;

    switch (_plantilla) {
      case PlantillaGeometrica.rectangulo:
        return a * b;
      case PlantillaGeometrica.formaL:
        // Área total del rectángulo envolvente – rectángulo recortado
        return (a * b) - ((a - ancho) * (b - ancho));
      case PlantillaGeometrica.formaU:
        return (a * ancho) + (b * ancho) + (c * ancho);
    }
  }

  // ── Añadir mesón al carrito ───────────────────────────────────────────────
  void _anadirMeson() {
    if (!_formKey.currentState!.validate()) return;

    final area = _calcularArea();
    final subtotal = area * _tipoPiedra.precioPorM2;

    final medidas = <String, double>{
      'ladoA': double.parse(_ctrlA.text),
      'ladoB': double.parse(_ctrlB.text),
      if (_ctrlC.text.isNotEmpty) 'ladoC': double.parse(_ctrlC.text),
      if (_ctrlAncho.text.isNotEmpty) 'ancho': double.parse(_ctrlAncho.text),
    };

    setState(() {
      _mesonesAgregados.add(_MesonItem(
        material: _tipoPiedra,
        geometria: _plantilla,
        medidas: medidas,
        areaTotal: area,
        precioSubtotal: subtotal,
      ));
    });

    _limpiarFormulario();

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.add_circle_outline,
                color: Colors.white, size: 16),
            const SizedBox(width: 8),
            Text(
              'Mesón agregado · ${area.toStringAsFixed(2)} m²  ·  Bs ${subtotal.toStringAsFixed(2)}',
              style: const TextStyle(fontSize: 13),
            ),
          ],
        ),
        backgroundColor: AppTheme.primaryBlue,
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        duration: const Duration(seconds: 2),
      ),
    );
  }

  // ── Guardar cotización completa ───────────────────────────────────────────
  Future<void> _guardarCotizacion() async {
    if (_mesonesAgregados.isEmpty) return;

    setState(() => _guardando = true);

    try {
      final payload = {
        'clienteId': widget.cliente.id,
        'comentarios': _ctrlComentarios.text.trim(),
        'detalles': _mesonesAgregados
            .map((m) => {
                  // Campos exactos del DetalleCotizacionCreateDto de C#
                  'nombreMaterial': m.material.label,
                  'geometria':      m.geometria.valor,
                  'ladoA':          m.medidas['ladoA'],
                  'ladoB':          m.medidas['ladoB'],
                  'ladoC':          m.medidas['ladoC'],   // null si no aplica
                  'ancho':          m.medidas['ancho'],   // null si no aplica
                  'precioPorM2':    m.material.precioPorM2,
                })
            .toList(),
      };

      await _service.crearCotizacion(payload);

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          duration: const Duration(seconds: 4),
          content: Row(
            children: [
              const Icon(Icons.check_circle_outline,
                  color: Colors.white, size: 18),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  '¡Cotización guardada! ${_mesonesAgregados.length} mesón(es)  ·  Total: Bs ${_precioTotal.toStringAsFixed(2)}',
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
          backgroundColor: const Color(0xFF27AE60),
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.all(16),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10)),
        ),
      );

      Navigator.pop(context);
    } catch (e) {
      if (!mounted) return;
      final msg = e.toString().replaceFirst('Exception: ', '');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.error_outline, color: Colors.white, size: 18),
              const SizedBox(width: 10),
              Expanded(
                  child: Text(msg, overflow: TextOverflow.ellipsis)),
            ],
          ),
          backgroundColor: AppTheme.errorColor,
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.all(16),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10)),
        ),
      );
    } finally {
      if (mounted) setState(() => _guardando = false);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────────
  // ── Helpers de UI ─────────────────────────────────────────────────────────
  // ─────────────────────────────────────────────────────────────────────────────

  String? _validarNumero(String? v, String nombre) {
    if (v == null || v.trim().isEmpty) return 'Ingresa $nombre';
    final n = double.tryParse(v);
    if (n == null || n <= 0) return '$nombre debe ser > 0';
    return null;
  }

  InputDecoration _inputDeco({
    required String label,
    required String hint,
    required IconData icon,
    String? suffix,
  }) =>
      InputDecoration(
        labelText: label,
        hintText: hint,
        hintStyle:
            const TextStyle(color: AppTheme.textSecondary, fontSize: 13),
        prefixIcon: Icon(icon, size: 20),
        suffixText: suffix,
        suffixStyle: const TextStyle(
            color: AppTheme.primaryBlue, fontWeight: FontWeight.w600),
        border:
            OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(
              color: AppTheme.primaryBlue.withValues(alpha: 0.5)),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide:
              const BorderSide(color: AppTheme.primaryBlue, width: 1.8),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppTheme.errorColor),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide:
              const BorderSide(color: AppTheme.errorColor, width: 1.8),
        ),
        filled: true,
        fillColor: AppTheme.backgroundWhite.withValues(alpha: 0.5),
      );

  InputDecoration _dropDeco(String label, IconData icon) => InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, size: 20),
        border:
            OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(
              color: AppTheme.primaryBlue.withValues(alpha: 0.5)),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide:
              const BorderSide(color: AppTheme.primaryBlue, width: 1.8),
        ),
        filled: true,
        fillColor: AppTheme.backgroundWhite.withValues(alpha: 0.5),
      );

  Widget _campoNumerico({
    required TextEditingController controller,
    required String label,
    required String hint,
    required IconData icon,
    required String? Function(String?) validator,
  }) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: TextFormField(
        controller: controller,
        keyboardType:
            const TextInputType.numberWithOptions(decimal: true),
        inputFormatters: [
          FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d*')),
        ],
        style:
            const TextStyle(color: AppTheme.textPrimary, fontSize: 15),
        decoration: _inputDeco(
            label: label, hint: hint, icon: icon, suffix: 'm'),
        validator: validator,
      ),
    );
  }

  Widget _buildCamposGeometricos() {
    return AnimatedSwitcher(
      duration: const Duration(milliseconds: 280),
      transitionBuilder: (child, anim) => FadeTransition(
        opacity: anim,
        child: SlideTransition(
          position: Tween<Offset>(
                  begin: const Offset(0, 0.05), end: Offset.zero)
              .animate(CurvedAnimation(
                  parent: anim, curve: Curves.easeOutCubic)),
          child: child,
        ),
      ),
      child: Column(
        key: ValueKey(_plantilla),
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _campoNumerico(
            controller: _ctrlA,
            label: 'Lado A',
            hint: 'Ej. 2.50',
            icon: Icons.swap_horiz_rounded,
            validator: (v) => _validarNumero(v, 'Lado A'),
          ),
          _campoNumerico(
            controller: _ctrlB,
            label: 'Lado B',
            hint: 'Ej. 1.20',
            icon: Icons.swap_vert_rounded,
            validator: (v) => _validarNumero(v, 'Lado B'),
          ),
          if (_plantilla == PlantillaGeometrica.formaU)
            _campoNumerico(
              controller: _ctrlC,
              label: 'Lado C',
              hint: 'Ej. 2.50',
              icon: Icons.swap_horiz_rounded,
              validator: (v) => _validarNumero(v, 'Lado C'),
            ),
          if (_plantilla != PlantillaGeometrica.rectangulo)
            _campoNumerico(
              controller: _ctrlAncho,
              label: 'Ancho',
              hint: 'Ej. 0.60',
              icon: Icons.straighten_outlined,
              validator: (v) => _validarNumero(v, 'Ancho'),
            ),
        ],
      ),
    );
  }

  Widget _buildDiagrama() {
    final Map<PlantillaGeometrica, String> d = {
      PlantillaGeometrica.rectangulo:
          '┌────────┐\n│        │  A × B\n└────────┘',
      PlantillaGeometrica.formaL:
          '┌──┐\n│  │ A\n│  └──┐\n│     │ B – Ancho\n└─────┘',
      PlantillaGeometrica.formaU:
          '┌──┐     ┌──┐\n│  │ A  C │  │\n│  └──┬──┘  │\n│     │ B   │\n└─────┴─────┘',
    };
    return AnimatedSwitcher(
      duration: const Duration(milliseconds: 200),
      child: Container(
        key: ValueKey(_plantilla),
        width: double.infinity,
        padding:
            const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: AppTheme.backgroundWhite,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(
              color: AppTheme.primaryBlue.withValues(alpha: 0.25)),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(Icons.schema_outlined,
                size: 13,
                color: AppTheme.primaryBlue.withValues(alpha: 0.55)),
            const SizedBox(width: 10),
            Text(
              d[_plantilla]!,
              style: const TextStyle(
                fontFamily: 'monospace',
                color: AppTheme.primaryBlue,
                fontSize: 12,
                height: 1.65,
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Tarjeta de un mesón en el carrito ────────────────────────────────────
  Widget _buildMesonCard(_MesonItem meson, int index) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      color: AppTheme.backgroundWhite.withValues(alpha: 0.8),
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: AppTheme.primaryBlue.withValues(alpha: 0.4),
        ),
      ),
      child: Padding(
        padding:
            const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        child: Row(
          children: [
            // Número
            Container(
              width: 28,
              height: 28,
              decoration: BoxDecoration(
                color: AppTheme.primaryBlue.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(7),
              ),
              child: Center(
                child: Text(
                  '${index + 1}',
                  style: const TextStyle(
                    color: AppTheme.primaryBlue,
                    fontWeight: FontWeight.w800,
                    fontSize: 13,
                  ),
                ),
              ),
            ),
            const SizedBox(width: 12),

            // Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${meson.material.label}  ·  ${meson.geometria.label}',
                    style: const TextStyle(
                      color: AppTheme.textPrimary,
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    _medidasLabel(meson),
                    style: const TextStyle(
                        color: AppTheme.textSecondary, fontSize: 12),
                  ),
                ],
              ),
            ),

            // Subtotal
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  '${meson.areaTotal.toStringAsFixed(2)} m²',
                  style: const TextStyle(
                    color: AppTheme.textSecondary,
                    fontSize: 12,
                  ),
                ),
                Text(
                  'Bs ${meson.precioSubtotal.toStringAsFixed(2)}',
                  style: const TextStyle(
                    color: AppTheme.primaryBlue,
                    fontWeight: FontWeight.w700,
                    fontSize: 14,
                  ),
                ),
              ],
            ),
            const SizedBox(width: 4),

            // Botón eliminar
            IconButton(
              icon: const Icon(Icons.delete_outline_rounded,
                  size: 20, color: AppTheme.errorColor),
              tooltip: 'Eliminar mesón',
              padding: EdgeInsets.zero,
              constraints: const BoxConstraints(),
              onPressed: () =>
                  setState(() => _mesonesAgregados.removeAt(index)),
            ),
          ],
        ),
      ),
    );
  }

  String _medidasLabel(_MesonItem m) {
    final parts = <String>[];
    if (m.medidas.containsKey('ladoA'))
      parts.add('A=${m.medidas['ladoA']!.toStringAsFixed(2)}');
    if (m.medidas.containsKey('ladoB'))
      parts.add('B=${m.medidas['ladoB']!.toStringAsFixed(2)}');
    if (m.medidas.containsKey('ladoC'))
      parts.add('C=${m.medidas['ladoC']!.toStringAsFixed(2)}');
    if (m.medidas.containsKey('ancho'))
      parts.add('Ancho=${m.medidas['ancho']!.toStringAsFixed(2)}');
    return parts.join('  ');
  }

  // ── Build principal ───────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final hayMesones = _mesonesAgregados.isNotEmpty;

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
              'Cotización',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              'Paso 2 · Agregar mesones',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
        // Badge con cantidad de mesones
        actions: [
          if (hayMesones)
            Padding(
              padding: const EdgeInsets.only(right: 16),
              child: Center(
                child: Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                        color: Colors.white.withValues(alpha: 0.4)),
                  ),
                  child: Text(
                    '${_mesonesAgregados.length} mesón(es)',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ── Banner cliente ─────────────────────────────────────────────
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(
                  horizontal: 16, vertical: 12),
              decoration: BoxDecoration(
                color: AppTheme.primaryBlue.withValues(alpha: 0.08),
                borderRadius: BorderRadius.circular(14),
                border: Border.all(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.3)),
              ),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(7),
                    decoration: BoxDecoration(
                      color: AppTheme.primaryBlue.withValues(alpha: 0.15),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.person_rounded,
                        color: AppTheme.primaryBlue, size: 18),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Cotizando para:',
                          style: TextStyle(
                            color: AppTheme.textSecondary,
                            fontSize: 10,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        Text(
                          widget.cliente.nombre,
                          style: const TextStyle(
                            color: AppTheme.primaryBlue,
                            fontSize: 16,
                            fontWeight: FontWeight.w800,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                        Text(
                          '📞 ${widget.cliente.telefonoCliente}',
                          style: const TextStyle(
                              color: AppTheme.textSecondary, fontSize: 11),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // ╔══════════════════════════════════════════════════════════╗
            // ║  SECCIÓN 1 — Formulario de mesón                         ║
            // ╚══════════════════════════════════════════════════════════╝
            Card(
              elevation: 4,
              color: AppTheme.cardColor,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(18),
                side: BorderSide(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.35)),
              ),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Cabecera de sección
                      Row(
                        children: [
                          Container(
                            width: 34,
                            height: 34,
                            decoration: BoxDecoration(
                              color: AppTheme.primaryBlue
                                  .withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(9),
                            ),
                            child: const Icon(
                              Icons.countertops_outlined,
                              color: AppTheme.primaryBlue,
                              size: 18,
                            ),
                          ),
                          const SizedBox(width: 10),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Agregar Mesón',
                                style: theme.textTheme.titleMedium
                                    ?.copyWith(
                                  color: AppTheme.textPrimary,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              Text(
                                'Material, geometría y medidas',
                                style: theme.textTheme.labelSmall
                                    ?.copyWith(color: AppTheme.textSecondary),
                              ),
                            ],
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      Divider(
                        color: AppTheme.primaryBlue.withValues(alpha: 0.4),
                        thickness: 1,
                      ),
                      const SizedBox(height: 16),

                      // Material
                      DropdownButtonFormField<TipoPiedra>(
                        value: _tipoPiedra,
                        dropdownColor: AppTheme.cardColor,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _dropDeco(
                            'Material de piedra', Icons.diamond_outlined),
                        items: TipoPiedra.values
                            .map((t) => DropdownMenuItem(
                                value: t,
                                child: Text(
                                    '${t.label}  (Bs ${t.precioPorM2.toStringAsFixed(0)}/m²)')))
                            .toList(),
                        onChanged: (v) =>
                            setState(() => _tipoPiedra = v!),
                      ),
                      const SizedBox(height: 14),

                      // Geometría
                      DropdownButtonFormField<PlantillaGeometrica>(
                        value: _plantilla,
                        dropdownColor: AppTheme.cardColor,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _dropDeco(
                            'Forma del mesón', Icons.grid_view_outlined),
                        items: PlantillaGeometrica.values
                            .map((p) => DropdownMenuItem(
                                value: p, child: Text(p.label)))
                            .toList(),
                        onChanged: (v) {
                          if (v == null) return;
                          setState(() {
                            _plantilla = v;
                            _ctrlA.clear();
                            _ctrlB.clear();
                            _ctrlC.clear();
                            _ctrlAncho.clear();
                          });
                        },
                      ),
                      const SizedBox(height: 12),
                      _buildDiagrama(),
                      const SizedBox(height: 16),

                      // Medidas dinámicas
                      _buildCamposGeometricos(),

                      // Botón Añadir
                      SizedBox(
                        width: double.infinity,
                        height: 50,
                        child: ElevatedButton.icon(
                          style: ElevatedButton.styleFrom(
                            backgroundColor:
                                AppTheme.primaryBlue.withValues(alpha: 0.9),
                            foregroundColor: AppTheme.textPrimary,
                            elevation: 3,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                          ),
                          onPressed: _anadirMeson,
                          icon: const Icon(Icons.add_rounded, size: 22),
                          label: Text(
                            'AÑADIR MESÓN',
                            style: theme.textTheme.labelLarge?.copyWith(
                              fontWeight: FontWeight.w800,
                              letterSpacing: 0.8,
                              color: AppTheme.textPrimary,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const SizedBox(height: 16),

            // ╔══════════════════════════════════════════════════════════╗
            // ║  SECCIÓN 2 — Lista de mesones agregados                  ║
            // ╚══════════════════════════════════════════════════════════╝
            if (hayMesones) ...[
              Card(
                elevation: 4,
                color: AppTheme.cardColor,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(18),
                  side: BorderSide(
                      color: const Color(0xFF27AE60).withValues(alpha: 0.3)),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Container(
                            width: 34,
                            height: 34,
                            decoration: BoxDecoration(
                              color: const Color(0xFF27AE60)
                                  .withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(9),
                            ),
                            child: const Icon(
                              Icons.shopping_cart_outlined,
                              color: Color(0xFF27AE60),
                              size: 18,
                            ),
                          ),
                          const SizedBox(width: 10),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Mesones Agregados',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: AppTheme.textPrimary,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              Text(
                                '${_mesonesAgregados.length} mesón(es) en esta cotización',
                                style: theme.textTheme.labelSmall?.copyWith(
                                    color: AppTheme.textSecondary),
                              ),
                            ],
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      Divider(
                        color: AppTheme.primaryBlue.withValues(alpha: 0.4),
                        thickness: 1,
                      ),
                      const SizedBox(height: 12),

                      // Lista
                      ListView.builder(
                        physics: const NeverScrollableScrollPhysics(),
                        shrinkWrap: true,
                        itemCount: _mesonesAgregados.length,
                        itemBuilder: (_, i) =>
                            _buildMesonCard(_mesonesAgregados[i], i),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),

              // ╔══════════════════════════════════════════════════════════╗
              // ║  SECCIÓN 3 — Comentarios + Total + Guardar               ║
              // ╚══════════════════════════════════════════════════════════╝
              Card(
                elevation: 4,
                color: AppTheme.cardColor,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(18),
                  side: BorderSide(
                      color: AppTheme.primaryBlue.withValues(alpha: 0.3)),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Comentarios generales
                      TextFormField(
                        controller: _ctrlComentarios,
                        keyboardType: TextInputType.multiline,
                        maxLines: 2,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _inputDeco(
                          label: 'Comentarios generales',
                          hint: 'Observaciones de la cotización (opcional)…',
                          icon: Icons.notes_rounded,
                        ),
                      ),
                      const SizedBox(height: 20),
                      Divider(
                        color: AppTheme.primaryBlue.withValues(alpha: 0.4),
                        thickness: 1,
                      ),
                      const SizedBox(height: 16),

                      // Total destacado
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          const Text(
                            'PRECIO TOTAL',
                            style: TextStyle(
                              color: AppTheme.textSecondary,
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              letterSpacing: 1.2,
                            ),
                          ),
                          FittedBox(
                            child: Text(
                              'Bs ${_precioTotal.toStringAsFixed(2)}',
                              style: const TextStyle(
                                color: AppTheme.primaryBlue,
                                fontSize: 32,
                                fontWeight: FontWeight.w900,
                                letterSpacing: -1,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 20),

                      // Botón guardar
                      SizedBox(
                        width: double.infinity,
                        height: 56,
                        child: ElevatedButton(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF27AE60),
                            foregroundColor: Colors.white,
                            elevation: 8,
                            shadowColor: const Color(0xFF27AE60)
                                .withValues(alpha: 0.4),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(14),
                            ),
                          ),
                          onPressed:
                              (hayMesones && !_guardando)
                                  ? _guardarCotizacion
                                  : null,
                          child: _guardando
                              ? const SizedBox(
                                  width: 24,
                                  height: 24,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2.5,
                                    valueColor: AlwaysStoppedAnimation<Color>(
                                        Colors.white),
                                  ),
                                )
                              : Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    const Icon(Icons.save_rounded, size: 22),
                                    const SizedBox(width: 10),
                                    Text(
                                      'GUARDAR COTIZACIÓN COMPLETA',
                                      style: theme.textTheme.labelLarge
                                          ?.copyWith(
                                        fontWeight: FontWeight.w800,
                                        letterSpacing: 0.8,
                                        color: Colors.white,
                                        fontSize: 14,
                                      ),
                                    ),
                                  ],
                                ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }
}
