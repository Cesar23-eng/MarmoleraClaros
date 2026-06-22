import 'package:flutter/material.dart';
import 'package:marmolera_app/core/theme/app_theme.dart';
import 'package:marmolera_app/features/ventas/screens/registro_cliente_screen.dart';
import 'package:marmolera_app/features/ventas/services/clientes_service.dart';

/// Pantalla de gestión de clientes.
/// Permite listar todos los clientes, buscar y crear uno nuevo.
class ClientesScreen extends StatefulWidget {
  const ClientesScreen({super.key});

  @override
  State<ClientesScreen> createState() => _ClientesScreenState();
}

class _ClientesScreenState extends State<ClientesScreen> {
  final ClientesService _service = ClientesService();
  final _searchCtrl = TextEditingController();

  List<ClienteDto> _todos = [];
  List<ClienteDto> _filtrados = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  // ── Cargar todos los clientes ────────────────────────────────────────────
  Future<void> _cargar() async {
    setState(() {
      _cargando = true;
      _error = null;
    });
    try {
      final lista = await _service.obtenerTodos();
      if (!mounted) return;
      setState(() {
        _todos = lista;
        _filtrados = lista;
        _cargando = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString().replaceFirst('Exception: ', '');
        _cargando = false;
      });
    }
  }

  // ── Filtrar localmente ───────────────────────────────────────────────────
  void _filtrar(String q) {
    final txt = q.trim().toLowerCase();
    setState(() {
      _filtrados = txt.isEmpty
          ? _todos
          : _todos
              .where((c) =>
                  c.nombre.toLowerCase().contains(txt) ||
                  c.telefonoCliente.contains(txt))
              .toList();
    });
  }

  // ── Navegar a crear cliente ──────────────────────────────────────────────
  Future<void> _crearCliente() async {
    final nuevo = await Navigator.push<ClienteDto>(
      context,
      MaterialPageRoute(builder: (_) => const RegistroClienteScreen()),
    );
    if (nuevo != null) {
      setState(() {
        _todos.insert(0, nuevo);
        _filtrar(_searchCtrl.text);
      });
    }
  }

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
              'Clientes',
              style: theme.textTheme.titleMedium?.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              '${_todos.length} registrados',
              style: theme.textTheme.labelSmall?.copyWith(
                color: Colors.white70,
                letterSpacing: 0.5,
              ),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded, color: Colors.white),
            tooltip: 'Actualizar',
            onPressed: _cargar,
          ),
          const SizedBox(width: 4),
        ],
      ),

      // ── FAB: nuevo cliente ───────────────────────────────────────────────
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _crearCliente,
        backgroundColor: AppTheme.primaryBlue,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.person_add_alt_1_rounded),
        label: const Text(
          'Nuevo Cliente',
          style: TextStyle(fontWeight: FontWeight.w700),
        ),
      ),

      body: Column(
        children: [
          // ── Barra de búsqueda ──────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            child: TextField(
              controller: _searchCtrl,
              onChanged: _filtrar,
              style:
                  const TextStyle(color: AppTheme.textPrimary, fontSize: 15),
              decoration: InputDecoration(
                hintText: 'Buscar por nombre o teléfono…',
                hintStyle: const TextStyle(
                    color: AppTheme.textSecondary, fontSize: 13),
                prefixIcon:
                    const Icon(Icons.search_rounded, size: 20),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.close_rounded, size: 18),
                        onPressed: () {
                          _searchCtrl.clear();
                          _filtrar('');
                        },
                      )
                    : null,
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12)),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide(
                    color: AppTheme.primaryBlue.withValues(alpha: 0.4),
                  ),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: const BorderSide(
                      color: AppTheme.primaryBlue, width: 1.8),
                ),
                filled: true,
                fillColor:
                    AppTheme.backgroundWhite.withValues(alpha: 0.5),
              ),
            ),
          ),

          // ── Cuerpo ─────────────────────────────────────────────────────
          Expanded(child: _buildBody(theme)),
        ],
      ),
    );
  }

  Widget _buildBody(ThemeData theme) {
    if (_cargando) {
      return const Center(
        child: CircularProgressIndicator(),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.wifi_off_rounded,
                  size: 56, color: AppTheme.textSecondary),
              const SizedBox(height: 16),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium
                    ?.copyWith(color: AppTheme.textSecondary),
              ),
              const SizedBox(height: 20),
              ElevatedButton.icon(
                onPressed: _cargar,
                icon: const Icon(Icons.refresh_rounded),
                label: const Text('Reintentar'),
              ),
            ],
          ),
        ),
      );
    }

    if (_filtrados.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                _searchCtrl.text.isNotEmpty
                    ? Icons.search_off_rounded
                    : Icons.people_outline_rounded,
                size: 64,
                color: AppTheme.textSecondary.withValues(alpha: 0.5),
              ),
              const SizedBox(height: 16),
              Text(
                _searchCtrl.text.isNotEmpty
                    ? 'Sin resultados para "${_searchCtrl.text}"'
                    : 'Aún no hay clientes registrados',
                textAlign: TextAlign.center,
                style: theme.textTheme.titleSmall?.copyWith(
                  color: AppTheme.textSecondary,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                _searchCtrl.text.isNotEmpty
                    ? 'Intenta con otro nombre o teléfono'
                    : 'Toca el botón + para agregar el primero',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodySmall
                    ?.copyWith(color: AppTheme.textSecondary),
              ),
            ],
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _cargar,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 8, 16, 100),
        itemCount: _filtrados.length,
        separatorBuilder: (_, __) => const SizedBox(height: 10),
        itemBuilder: (_, i) => _ClienteTile(
          cliente: _filtrados[i],
          theme: theme,
        ),
      ),
    );
  }
}

// ── Tarjeta de cliente ───────────────────────────────────────────────────────
class _ClienteTile extends StatelessWidget {
  final ClienteDto cliente;
  final ThemeData theme;

  const _ClienteTile({required this.cliente, required this.theme});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppTheme.cardColor,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: AppTheme.primaryBlue.withValues(alpha: 0.2),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: ListTile(
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
        leading: CircleAvatar(
          backgroundColor: AppTheme.primaryBlue.withValues(alpha: 0.12),
          child: Text(
            cliente.nombre.isNotEmpty
                ? cliente.nombre[0].toUpperCase()
                : '?',
            style: const TextStyle(
              color: AppTheme.primaryBlue,
              fontWeight: FontWeight.w700,
              fontSize: 18,
            ),
          ),
        ),
        title: Text(
          cliente.nombre,
          style: theme.textTheme.titleSmall?.copyWith(
            color: AppTheme.textPrimary,
            fontWeight: FontWeight.w700,
          ),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (cliente.telefonoCliente.isNotEmpty)
              Row(
                children: [
                  const Icon(Icons.phone_outlined,
                      size: 13, color: AppTheme.textSecondary),
                  const SizedBox(width: 4),
                  Text(
                    cliente.telefonoCliente,
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: AppTheme.textSecondary,
                    ),
                  ),
                ],
              ),
            if (cliente.direccionCliente.isNotEmpty)
              Row(
                children: [
                  const Icon(Icons.location_on_outlined,
                      size: 13, color: AppTheme.textSecondary),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      cliente.direccionCliente,
                      style: theme.textTheme.labelSmall?.copyWith(
                        color: AppTheme.textSecondary,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ],
              ),
          ],
        ),
        trailing: const Icon(
          Icons.chevron_right_rounded,
          color: AppTheme.textSecondary,
        ),
      ),
    );
  }
}
