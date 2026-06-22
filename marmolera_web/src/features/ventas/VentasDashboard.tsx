import { useEffect, useState } from 'react';
import { FileText, Users, TrendingUp, Search } from 'lucide-react';
import type { CotizacionResponseDto, EstadoCotizacion } from '../../types/ventas';
import { cotizacionesService } from './services/cotizacionesService';
import { useClientes } from './hooks/useClientes';
import EstadoBadge from './components/EstadoBadge';
import NuevaCotizacionModal from './components/NuevaCotizacionModal';
import DetalleCotizacionModal from './components/DetalleCotizacionModal';
import { useAuthStore } from '../../core/store/useAuthStore';

const ESTADOS: { value: EstadoCotizacion | 'Todos'; label: string }[] = [
  { value: 'Todos',       label: 'Todos' },
  { value: 'Cotizado',    label: 'Cotizado' },
  { value: 'Aprobado',    label: 'Aprobado' },
  { value: 'EnProduccion',label: 'En Producción' },
  { value: 'Finalizado',  label: 'Finalizado' },
];

export default function VentasDashboard() {
  const { role } = useAuthStore();
  const { clientes } = useClientes();

  const [cotizaciones, setCotizaciones] = useState<CotizacionResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filtroEstado, setFiltroEstado] = useState<EstadoCotizacion | 'Todos'>('Todos');
  const [busqueda, setBusqueda] = useState('');
  const [modalNueva, setModalNueva] = useState(false);
  const [cotizacionDetalle, setCotizacionDetalle] = useState<CotizacionResponseDto | null>(null);

  const cargarCotizaciones = async () => {
    try {
      setLoading(true);
      const data = role === 'Admin'
        ? await cotizacionesService.getTodas()
        : await cotizacionesService.getMias();
      setCotizaciones(data);
    } catch {
      /* silencioso en mock */
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { cargarCotizaciones(); }, []);

  const cotizacionesFiltradas = cotizaciones
    .filter((c) => filtroEstado === 'Todos' || c.estado === filtroEstado)
    .filter((c) =>
      busqueda === '' ||
      c.cliente.nombreCompleto.toLowerCase().includes(busqueda.toLowerCase()) ||
      String(c.id).includes(busqueda)
    );

  // KPIs
  const totalCotizaciones = cotizaciones.length;
  const clientesUnicos    = new Set(cotizaciones.map((c) => c.cliente.id)).size;
  const totalVentas       = cotizaciones
    .filter((c) => c.estado === 'Finalizado')
    .reduce((sum, c) => sum + c.precioTotal, 0);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Dashboard de Ventas</h1>
          <p className="text-slate-500 text-sm mt-1">Gestión de cotizaciones y clientes</p>
        </div>
        <button
          onClick={() => setModalNueva(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2.5 rounded-lg transition"
        >
          + Nueva Cotización
        </button>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {[
          { label: 'Cotizaciones',      value: totalCotizaciones, icon: FileText,   color: 'blue' },
          { label: 'Clientes',          value: clientesUnicos,    icon: Users,      color: 'emerald' },
          { label: 'Facturado (Final)', value: `Bs ${totalVentas.toFixed(0)}`, icon: TrendingUp, color: 'violet' },
        ].map(({ label, value, icon: Icon, color }) => (
          <div key={label} className="bg-white rounded-xl border border-slate-200 p-5 flex items-center gap-4">
            <div className={`w-11 h-11 rounded-lg flex items-center justify-center shrink-0 ${
              color === 'blue' ? 'bg-blue-50 text-blue-600' :
              color === 'emerald' ? 'bg-emerald-50 text-emerald-600' :
              'bg-violet-50 text-violet-600'
            }`}>
              <Icon size={22} />
            </div>
            <div>
              <p className="text-xs font-medium text-slate-500">{label}</p>
              <p className="text-2xl font-bold text-slate-800 mt-0.5 tabular-nums">{value}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Filtros */}
      <div className="bg-white rounded-xl border border-slate-200">
        <div className="px-5 py-3.5 border-b border-slate-100 flex flex-wrap items-center gap-3">
          {/* Búsqueda */}
          <div className="relative flex-1 min-w-[200px]">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por cliente o # cotización..."
              value={busqueda}
              onChange={(e) => setBusqueda(e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            />
          </div>

          {/* Filtro estado */}
          <div className="flex gap-2">
            {ESTADOS.map((e) => (
              <button
                key={e.value}
                onClick={() => setFiltroEstado(e.value)}
                className={`px-3 py-1.5 text-xs font-medium rounded-lg transition ${
                  filtroEstado === e.value
                    ? 'bg-blue-600 text-white'
                    : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
                }`}
              >
                {e.label}
              </button>
            ))}
          </div>
        </div>

        {/* Tabla */}
        {loading ? (
          <div className="flex items-center justify-center h-52 text-slate-400 text-sm">Cargando...</div>
        ) : cotizacionesFiltradas.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-52 gap-2 text-slate-400">
            <FileText size={36} className="opacity-20" />
            <p className="text-sm">No hay cotizaciones{filtroEstado !== 'Todos' ? ` con estado "${filtroEstado}"` : ''}.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-medium text-slate-500">#</th>
                  <th className="px-5 py-3 text-left text-xs font-medium text-slate-500">Cliente</th>
                  <th className="px-5 py-3 text-left text-xs font-medium text-slate-500">Mesones</th>
                  <th className="px-5 py-3 text-left text-xs font-medium text-slate-500">Fecha</th>
                  <th className="px-5 py-3 text-left text-xs font-medium text-slate-500">Estado</th>
                  <th className="px-5 py-3 text-right text-xs font-medium text-slate-500">Total</th>
                  <th className="px-5 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {cotizacionesFiltradas.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50 transition">
                    <td className="px-5 py-3.5 font-medium text-slate-700">#{c.id}</td>
                    <td className="px-5 py-3.5">
                      <p className="font-medium text-slate-800">{c.cliente.nombreCompleto}</p>
                      <p className="text-xs text-slate-500">{c.cliente.telefono}</p>
                    </td>
                    <td className="px-5 py-3.5 text-slate-600">{c.detalles.length}</td>
                    <td className="px-5 py-3.5 text-slate-500">
                      {new Date(c.fechaCreacion).toLocaleDateString('es-BO')}
                    </td>
                    <td className="px-5 py-3.5">
                      <EstadoBadge estado={c.estado} />
                    </td>
                    <td className="px-5 py-3.5 text-right tabular-nums font-semibold text-slate-800">
                      Bs {c.precioTotal.toFixed(2)}
                    </td>
                    <td className="px-5 py-3.5 text-right">
                      <button
                        onClick={() => setCotizacionDetalle(c)}
                        className="text-xs text-blue-600 hover:text-blue-800 font-medium"
                      >
                        Ver detalle
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modales */}
      {modalNueva && (
        <NuevaCotizacionModal
          clientes={clientes}
          onClose={() => setModalNueva(false)}
          onCreada={() => { setModalNueva(false); cargarCotizaciones(); }}
        />
      )}

      {cotizacionDetalle && (
        <DetalleCotizacionModal
          cotizacion={cotizacionDetalle}
          onClose={() => setCotizacionDetalle(null)}
          onActualizada={() => { setCotizacionDetalle(null); cargarCotizaciones(); }}
        />
      )}
    </div>
  );
}
