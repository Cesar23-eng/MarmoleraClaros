import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/ventas/services/clientes_service.dart';

/// Pantalla dedicada al registro de un cliente nuevo.
/// Al guardar con éxito retorna automáticamente con el [ClienteDto] creado.
class RegistroClienteScreen extends StatefulWidget {
  const RegistroClienteScreen({super.key});

  @override
  State<RegistroClienteScreen> createState() => _RegistroClienteScreenState();
}

class _RegistroClienteScreenState extends State<RegistroClienteScreen> {
  final _formKey      = GlobalKey<FormState>();
  final _ctrlNombre   = TextEditingController();
  final _ctrlTelefono = TextEditingController();
  final _ctrlDir      = TextEditingController();

  bool _guardando = false;

  final ClientesService _service = ClientesService();

  @override
  void dispose() {
    _ctrlNombre.dispose();
    _ctrlTelefono.dispose();
    _ctrlDir.dispose();
    super.dispose();
  }

  // ── Guardar cliente ──────────────────────────────────────────────────────────
  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _guardando = true);

    try {
      final cliente = await _service.registrarCliente(
        nombre:           _ctrlNombre.text.trim(),
        telefonoCliente:  _ctrlTelefono.text.trim(),
        direccionCliente: _ctrlDir.text.trim(),
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.check_circle_outline,
                  color: Colors.white, size: 18),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Cliente "${cliente.nombre}" registrado correctamente.',
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
          backgroundColor: const Color(0xFF27AE60),
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.all(16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(10),
          ),
        ),
      );

      // Regresar a la pantalla anterior y entregar el cliente creado
      Navigator.of(context).pop(cliente);
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
                child: Text(msg, overflow: TextOverflow.ellipsis),
              ),
            ],
          ),
          backgroundColor: AppTheme.errorColor,
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.all(16),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(10),
          ),
        ),
      );
    } finally {
      if (mounted) setState(() => _guardando = false);
    }
  }

  // ── Decoración estándar de campos ────────────────────────────────────────────
  InputDecoration _deco({
    required String label,
    required String hint,
    required IconData icon,
  }) =>
      InputDecoration(
        labelText: label,
        hintText: hint,
        hintStyle:
            const TextStyle(color: AppTheme.textSecondary, fontSize: 13),
        prefixIcon: Icon(icon, size: 20),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
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

  // ── Build ────────────────────────────────────────────────────────────────────
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
              'Nuevo Cliente',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              'Datos de contacto',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 4),
              // ── Encabezado ──────────────────────────────────────────────────
              Text(
                'Registrar Cliente',
                style: theme.textTheme.headlineSmall?.copyWith(
                  color: AppTheme.textPrimary,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                'Completa los datos para agregar al cliente al sistema.',
                style: theme.textTheme.bodySmall
                    ?.copyWith(color: AppTheme.textSecondary),
              ),
              const SizedBox(height: 28),

              // ── Card principal ───────────────────────────────────────────────
              Card(
                elevation: 4,
                color: AppTheme.cardColor,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(18),
                  side: BorderSide(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.35),
                  ),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Ícono + títulos de sección
                      Row(
                        children: [
                          Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(
                              color:
                                  AppTheme.primaryBlue.withValues(alpha: 0.12),
                              borderRadius: BorderRadius.circular(9),
                            ),
                            child: const Icon(
                              Icons.person_add_alt_1_rounded,
                              color: AppTheme.primaryBlue,
                              size: 20,
                            ),
                          ),
                          const SizedBox(width: 12),
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Información del Cliente',
                                style: theme.textTheme.titleMedium?.copyWith(
                                  color: AppTheme.textPrimary,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                              Text(
                                'Nombre, teléfono y dirección',
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

                      // ── Nombre ──────────────────────────────────────────────
                      TextFormField(
                        controller: _ctrlNombre,
                        keyboardType: TextInputType.name,
                        textCapitalization: TextCapitalization.words,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _deco(
                          label: 'Nombre completo',
                          hint: 'Ej. Juan Pérez',
                          icon: Icons.person_outlined,
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty)
                            ? 'Ingresa el nombre del cliente'
                            : null,
                      ),
                      const SizedBox(height: 16),

                      // ── Teléfono ─────────────────────────────────────────────
                      TextFormField(
                        controller: _ctrlTelefono,
                        keyboardType: TextInputType.phone,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _deco(
                          label: 'Teléfono',
                          hint: 'Ej. +591 77712345',
                          icon: Icons.phone_outlined,
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty)
                            ? 'Ingresa el teléfono del cliente'
                            : null,
                      ),
                      const SizedBox(height: 16),

                      // ── Dirección ────────────────────────────────────────────
                      TextFormField(
                        controller: _ctrlDir,
                        keyboardType: TextInputType.streetAddress,
                        textCapitalization: TextCapitalization.sentences,
                        style: const TextStyle(
                            color: AppTheme.textPrimary, fontSize: 15),
                        decoration: _deco(
                          label: 'Dirección',
                          hint: 'Ej. Av. Beni #123, Santa Cruz',
                          icon: Icons.location_on_outlined,
                        ),
                        validator: (v) => (v == null || v.trim().isEmpty)
                            ? 'Ingresa la dirección del cliente'
                            : null,
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 28),

              // ── Botón guardar ────────────────────────────────────────────────
              SizedBox(
                width: double.infinity,
                height: 54,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.primaryBlue,
                    foregroundColor: AppTheme.backgroundWhite,
                    elevation: 6,
                    shadowColor: AppTheme.primaryBlue.withValues(alpha: 0.35),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                  ),
                  onPressed: _guardando ? null : _guardar,
                  child: _guardando
                      ? const SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(
                            strokeWidth: 2.5,
                            valueColor: AlwaysStoppedAnimation<Color>(
                                AppTheme.backgroundWhite),
                          ),
                        )
                      : Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const Icon(Icons.save_rounded, size: 22),
                            const SizedBox(width: 10),
                            Text(
                              'GUARDAR CLIENTE',
                              style: theme.textTheme.labelLarge?.copyWith(
                                fontWeight: FontWeight.w800,
                                letterSpacing: 1.0,
                                color: AppTheme.backgroundWhite,
                                fontSize: 15,
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
    );
  }
}
