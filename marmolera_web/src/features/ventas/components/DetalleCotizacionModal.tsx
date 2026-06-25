import { useState } from 'react';
import { X, CheckCircle, Pencil, Trash2, AlertTriangle } from 'lucide-react';
import type { CotizacionResponseDto, ClienteResponseDto } from '../../../types/ventas';
import EstadoBadge from './EstadoBadge';
import { cotizacionesService } from '../services/cotizacionesService';
import { useAuthStore } from '../../../core/store/useAuthStore';
import { calcularArea } from './AreaPreview';
import MesonForm from './MesonForm';
import NuevoClienteModal from './NuevoClienteModal';
import { UserPlus, Plus } from 'lucide-react';
import type { DetalleCotizacionCreateDto } from '../../../types/ventas';

interface Props {
  cotizacion: CotizacionResponseDto;
  clientes: ClienteResponseDto[];
  onClose: () => void;
  onActualizada: () => void;
}

const mesonVacio = (): DetalleCotizacionCreateDto => ({
  nombreMaterial: '',
  geometria: 'Rectangulo',
  ladoA: 0,
  ladoB: 0,
  ladoC: undefined,
  ancho: undefined,
  precioPorM2: 0,
});

export default function DetalleCotizacionModal({ cotizacion, clientes: clientesIniciales, onClose, onActualizada }: Props) {
  const { role } = useAuthStore();

  // ── Permisos
  const puedeEditarEliminar =
    cotizacion.estado === 'Cotizado' && (role === 'Admin' || role === 'Ventas');
  const puedeAprobar =
    cotizacion.estado === 'Cotizado' && (role === 'Admin' || role === 'Ventas');

  // ── Modos de vista
  const [modo, setModo] = useState<'ver' | 'editar' | 'confirmarEliminar'>('ver');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // ── Estado del formulario de edición
  const [clientes, setClientes] = useState<ClienteResponseDto[]>(clientesIniciales);
  const [clienteId, setClienteId] = useState<number>(cotizacion.cliente.id);
  const [comentarios, setComentarios] = useState(cotizacion.comentarios ?? '');
  const [detalles, setDetalles] = useState<DetalleCotizacionCreateDto[]>(
    cotizacion.detalles.map((d) => ({
      nombreMaterial: d.nombreMaterial,
      geometria: d.geometria as DetalleCotizacionCreateDto['geometria'],
      ladoA: 0,
      ladoB: 0,
      ladoC: undefined,
      ancho: undefined,
      precioPorM2: d.precioPorM2,
    }))
  );
  const [modalCliente, setModalCliente] = useState(false);

  const totalEstimado = detalles.reduce((sum, d) => {
    const area = calcularArea({ geometria: d.geometria, ladoA: d.ladoA, ladoB: d.ladoB, ladoC: d.ladoC, ancho: d.ancho });
    return sum + area * (d.precioPorM2 || 0);
  }, 0);

  const handleChange = (index: number, campo: keyof DetalleCotizacionCreateDto, valor: string | number | undefined) => {
    setDetalles((prev) => {
      const copia = [...prev];
      copia[index] = { ...copia[index], [campo]: valor };
      if (campo === 'geometria') {
        if (valor === 'Rectangulo') { copia[index].ladoC = undefined; copia[index].ancho = undefined; }
        if (valor === 'Forma_L')   { copia[index].ladoC = undefined; }
      }
      return copia;
    });
  };

  const handleClienteCreado = (clienteNuevo: ClienteResponseDto) => {
    setClientes((prev) => [...prev, clienteNuevo]);
    setClienteId(clienteNuevo.id);
    setModalCliente(false);
  };

  // ── Guardar edición
  const handleGuardar = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!clienteId) { setError('Selecciona un cliente.'); return; }
    const payload = {
      ClienteId:   clienteId,
      Comentarios: comentarios || null,
      Detalles: detalles.map((d) => ({
        NombreMaterial: d.nombreMaterial,
        Geometria:      d.geometria,
        LadoA:          d.ladoA,
        LadoB:          d.ladoB,
        LadoC:          d.ladoC ?? null,
        Ancho:          d.ancho ?? null,
        PrecioPorM2:    d.precioPorM2,
      })),
    };
    try {
      setLoading(true);
      await cotizacionesService.editarRaw(cotizacion.id, payload);
      onActualizada();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al guardar los cambios.');
    } finally {
      setLoading(false);
    }
  };

  // ── Eliminar
  const handleEliminar = async () => {
    try {
      setLoading(true);
      await cotizacionesService.eliminar(cotizacion.id);
      onActualizada();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al eliminar la cotización.');
      setModo('ver');
    } finally {
      setLoading(false);
    }
  };

  // ── Aprobar
  const handleAprobar = async () => {
    try {
      setLoading(true);
      await cotizacionesService.aprobar(cotizacion.id);
      onActualizada();
    } finally {
      setLoading(false);
    }
  };

  // ════════════════════════════ RENDER ════════════════════════════

  // ── Confirmar eliminación
  if (modo === 'confirmarEliminar') {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div className="w-full max-w-md bg-white rounded-2xl shadow-xl p-6 space-y-4">
          <div className="flex items-center gap-3 text-red-600">
            <AlertTriangle size={24} />
            <h2 className="text-lg font-bold">Eliminar Cotización #{cotizacion.id}</h2>
          </div>
          <p className="text-sm text-slate-600">
            ¿Estás seguro que deseas eliminar esta cotización de
            <span className="font-semibold"> {cotizacion.cliente.nombreCompleto}</span>?
            Esta acción no se puede deshacer.
          </p>
          {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</p>}
          <div className="flex justify-end gap-3 pt-2">
            <button
              onClick={() => setModo('ver')}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
            >
              Cancelar
            </button>
            <button
              onClick={handleEliminar}
              disabled={loading}
              className="px-5 py-2 text-sm font-semibold bg-red-600 text-white rounded-lg hover:bg-red-700 transition disabled:opacity-60"
            >
              {loading ? 'Eliminando...' : 'Sí, eliminar'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  // ── Modo editar
  if (modo === 'editar') {
    return (
      <>
        <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 overflow-y-auto py-8 px-4">
          <div className="w-full max-w-3xl bg-white rounded-2xl shadow-xl">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 sticky top-0 bg-white z-10">
              <h2 className="text-lg font-bold text-slate-800">Editar Cotización #{cotizacion.id}</h2>
              <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleGuardar} className="p-6 space-y-5">
              {error && (
                <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-3 text-sm">{error}</div>
              )}

              {/* Cliente */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Cliente *</label>
                  <div className="flex gap-2">
                    <select
                      value={clienteId}
                      onChange={(e) => setClienteId(Number(e.target.value))}
                      className="flex-1 px-3 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                      required
                    >
                      <option value={0}>Seleccionar cliente...</option>
                      {clientes.map((c) => (
                        <option key={c.id} value={c.id}>{c.nombreCompleto} — {c.telefono}</option>
                      ))}
                    </select>
                    <button
                      type="button"
                      onClick={() => setModalCliente(true)}
                      className="flex items-center gap-1 px-3 py-2.5 text-sm font-medium text-blue-600 border border-blue-200 rounded-lg hover:bg-blue-50 transition shrink-0"
                    >
                      <UserPlus size={15} /> Nuevo
                    </button>
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Comentarios</label>
                  <input
                    type="text"
                    value={comentarios}
                    onChange={(e) => setComentarios(e.target.value)}
                    placeholder="Notas adicionales..."
                    className="w-full px-3 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              {/* Mesones */}
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-semibold text-slate-700">Mesones</h3>
                  <button
                    type="button"
                    onClick={() => setDetalles((p) => [...p, mesonVacio()])}
                    className="flex items-center gap-1.5 text-sm text-blue-600 hover:text-blue-800 font-medium"
                  >
                    <Plus size={15} /> Agregar mesón
                  </button>
                </div>
                {detalles.map((d, i) => (
                  <MesonForm
                    key={i}
                    index={i}
                    detalle={d}
                    onChange={handleChange}
                    onRemove={(idx) => setDetalles((p) => p.filter((_, x) => x !== idx))}
                    canRemove={detalles.length > 1}
                  />
                ))}
              </div>

              {/* Total */}
              <div className="flex justify-end">
                <div className="bg-slate-50 border border-slate-200 rounded-xl px-5 py-3 text-right">
                  <p className="text-xs text-slate-500">Total estimado</p>
                  <p className="text-2xl font-bold text-slate-800">Bs {totalEstimado.toFixed(2)}</p>
                </div>
              </div>

              <div className="flex justify-end gap-3 pt-2">
                <button type="button" onClick={() => setModo('ver')} className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800">
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="px-5 py-2 text-sm font-semibold bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-60"
                >
                  {loading ? 'Guardando...' : 'Guardar cambios'}
                </button>
              </div>
            </form>
          </div>
        </div>

        {modalCliente && (
          <NuevoClienteModal
            onClose={() => setModalCliente(false)}
            onCreado={handleClienteCreado}
          />
        )}
      </>
    );
  }

  // ── Modo ver (default)
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 sticky top-0 bg-white z-10">
          <div className="flex items-center gap-3">
            <h2 className="text-lg font-bold text-slate-800">Cotización #{cotizacion.id}</h2>
            <EstadoBadge estado={cotizacion.estado} />
          </div>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
            <X size={20} />
          </button>
        </div>

        <div className="p-6 space-y-5">
          {/* Info cabecera */}
          <div className="grid grid-cols-2 gap-4 bg-slate-50 rounded-xl p-4">
            <div>
              <p className="text-xs text-slate-500">Cliente</p>
              <p className="font-semibold text-slate-800">{cotizacion.cliente.nombreCompleto}</p>
              <p className="text-sm text-slate-500">{cotizacion.cliente.telefono}</p>
            </div>
            <div>
              <p className="text-xs text-slate-500">Fecha</p>
              <p className="font-semibold text-slate-800">
                {new Date(cotizacion.fechaCreacion).toLocaleDateString('es-BO')}
              </p>
              {cotizacion.comentarios && (
                <p className="text-sm text-slate-500 mt-1">{cotizacion.comentarios}</p>
              )}
            </div>
          </div>

          {/* Tabla mesones */}
          <div>
            <h3 className="text-sm font-semibold text-slate-700 mb-2">Mesones</h3>
            <div className="border border-slate-200 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-slate-50">
                  <tr>
                    <th className="px-4 py-2.5 text-left text-xs font-medium text-slate-500">#</th>
                    <th className="px-4 py-2.5 text-left text-xs font-medium text-slate-500">Material</th>
                    <th className="px-4 py-2.5 text-left text-xs font-medium text-slate-500">Geometría</th>
                    <th className="px-4 py-2.5 text-right text-xs font-medium text-slate-500">Área (m²)</th>
                    <th className="px-4 py-2.5 text-right text-xs font-medium text-slate-500">Precio/m²</th>
                    <th className="px-4 py-2.5 text-right text-xs font-medium text-slate-500">Subtotal</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {cotizacion.detalles.map((d, i) => (
                    <tr key={d.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3 text-slate-500">{i + 1}</td>
                      <td className="px-4 py-3 font-medium text-slate-800">{d.nombreMaterial}</td>
                      <td className="px-4 py-3 text-slate-600">{d.geometria}</td>
                      <td className="px-4 py-3 text-right tabular-nums text-slate-700">{d.areaTotal.toFixed(4)}</td>
                      <td className="px-4 py-3 text-right tabular-nums text-slate-700">Bs {d.precioPorM2.toFixed(2)}</td>
                      <td className="px-4 py-3 text-right tabular-nums font-semibold text-slate-800">Bs {d.precioSubtotal.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-slate-50">
                  <tr>
                    <td colSpan={5} className="px-4 py-3 text-right text-sm font-semibold text-slate-700">Total</td>
                    <td className="px-4 py-3 text-right text-base font-bold text-slate-800">Bs {cotizacion.precioTotal.toFixed(2)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>

          {/* Acciones */}
          <div className="flex items-center justify-between pt-1">
            <div className="flex gap-2">
              {puedeEditarEliminar && (
                <>
                  <button
                    onClick={() => setModo('editar')}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-blue-600 border border-blue-200 rounded-lg hover:bg-blue-50 transition"
                  >
                    <Pencil size={14} /> Editar
                  </button>
                  <button
                    onClick={() => setModo('confirmarEliminar')}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-red-600 border border-red-200 rounded-lg hover:bg-red-50 transition"
                  >
                    <Trash2 size={14} /> Eliminar
                  </button>
                </>
              )}
            </div>
            <div className="flex gap-2">
              <button
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
              >
                Cerrar
              </button>
              {puedeAprobar && (
                <button
                  onClick={handleAprobar}
                  disabled={loading}
                  className="flex items-center gap-2 px-5 py-2 text-sm font-semibold bg-green-600 text-white rounded-lg hover:bg-green-700 transition disabled:opacity-60"
                >
                  <CheckCircle size={16} />
                  {loading ? 'Aprobando...' : 'Aprobar'}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
