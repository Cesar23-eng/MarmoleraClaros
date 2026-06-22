import { useState } from 'react';
import { X, Plus } from 'lucide-react';
import type {
  CotizacionCreateDto,
  DetalleCotizacionCreateDto,
  ClienteResponseDto,
} from '../../../types/ventas';
import { cotizacionesService } from '../services/cotizacionesService';
import { calcularArea } from './AreaPreview';
import MesonForm from './MesonForm';

const mesonVacio = (): DetalleCotizacionCreateDto => ({
  nombreMaterial: '',
  geometria: 'Rectangulo',
  ladoA: 0,
  ladoB: 0,
  ladoC: undefined,
  ancho: undefined,
  precioPorM2: 0,
});

interface Props {
  clientes: ClienteResponseDto[];
  onClose: () => void;
  onCreada: () => void;
}

export default function NuevaCotizacionModal({ clientes, onClose, onCreada }: Props) {
  const [clienteId, setClienteId] = useState<number>(0);
  const [comentarios, setComentarios] = useState('');
  const [detalles, setDetalles] = useState<DetalleCotizacionCreateDto[]>([mesonVacio()]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const totalEstimado = detalles.reduce((sum, d) => {
    const area = calcularArea({ geometria: d.geometria, ladoA: d.ladoA, ladoB: d.ladoB, ladoC: d.ladoC, ancho: d.ancho });
    return sum + area * (d.precioPorM2 || 0);
  }, 0);

  const handleChange = (
    index: number,
    campo: keyof DetalleCotizacionCreateDto,
    valor: string | number | undefined
  ) => {
    setDetalles((prev) => {
      const copia = [...prev];
      copia[index] = { ...copia[index], [campo]: valor };
      // Limpiar LadoC y Ancho si cambia geometría
      if (campo === 'geometria') {
        if (valor === 'Rectangulo') {
          copia[index].ladoC = undefined;
          copia[index].ancho = undefined;
        }
        if (valor === 'Forma_L') {
          copia[index].ladoC = undefined;
        }
      }
      return copia;
    });
  };

  const agregarMeson = () => setDetalles((p) => [...p, mesonVacio()]);
  const eliminarMeson = (i: number) => setDetalles((p) => p.filter((_, idx) => idx !== i));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!clienteId) { setError('Selecciona un cliente.'); return; }

    const dto: CotizacionCreateDto = {
      clienteId,
      comentarios: comentarios || undefined,
      detalles,
    };

    try {
      setLoading(true);
      await cotizacionesService.crear(dto);
      onCreada();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al crear la cotización.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 overflow-y-auto py-8 px-4">
      <div className="w-full max-w-3xl bg-white rounded-2xl shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 className="text-lg font-bold text-slate-800">Nueva Cotización</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {error && (
            <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-3 text-sm">
              {error}
            </div>
          )}

          {/* Cliente */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Cliente *</label>
              <select
                value={clienteId}
                onChange={(e) => setClienteId(Number(e.target.value))}
                className="w-full px-3 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
                required
              >
                <option value={0}>Seleccionar cliente...</option>
                {clientes.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nombreCompleto} — {c.telefono}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Comentarios</label>
              <input
                type="text"
                value={comentarios}
                onChange={(e) => setComentarios(e.target.value)}
                placeholder="Notas adicionales..."
                className="w-full px-3 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
              />
            </div>
          </div>

          {/* Mesones */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-semibold text-slate-700">Mesones</h3>
              <button
                type="button"
                onClick={agregarMeson}
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
                onRemove={eliminarMeson}
                canRemove={detalles.length > 1}
              />
            ))}
          </div>

          {/* Total estimado */}
          <div className="flex justify-end">
            <div className="bg-slate-50 border border-slate-200 rounded-xl px-5 py-3 text-right">
              <p className="text-xs text-slate-500">Total estimado</p>
              <p className="text-2xl font-bold text-slate-800">Bs {totalEstimado.toFixed(2)}</p>
            </div>
          </div>

          {/* Acciones */}
          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800 transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-5 py-2 text-sm font-semibold bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-60"
            >
              {loading ? 'Guardando...' : 'Crear Cotización'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
