import { useState } from 'react';
import { ChevronDown, ChevronUp, PlayCircle, CheckCircle2 } from 'lucide-react';
import type { CotizacionKanbanDto } from '../../../types/fabrica';

interface Props {
  orden: CotizacionKanbanDto;
  columna: 'porIniciar' | 'enProduccion' | 'finalizado';
  onAccion: (id: number) => Promise<void>;
}

export default function KanbanCard({ orden, columna, onAccion }: Props) {
  const [expandido, setExpandido] = useState(false);
  const [loading, setLoading]     = useState(false);

  const handleAccion = async () => {
    try {
      setLoading(true);
      await onAccion(orden.id);
    } finally {
      setLoading(false);
    }
  };

  const fechaLabel = orden.fechaAprobacion
    ? new Date(orden.fechaAprobacion).toLocaleDateString('es-BO', { day: '2-digit', month: 'short' })
    : '';

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
      {/* Cabecera tarjeta */}
      <div className="px-4 pt-4 pb-3">
        <div className="flex items-start justify-between gap-2">
          <div>
            <p className="font-semibold text-slate-800 text-sm leading-tight">{orden.nombreCliente}</p>
            <p className="text-xs text-slate-500 mt-0.5">{orden.telefono}</p>
          </div>
          <span className="text-xs font-bold text-slate-400 shrink-0">#{orden.id}</span>
        </div>

        {/* Meta */}
        <div className="flex items-center gap-3 mt-2.5">
          <span className="text-xs bg-slate-100 text-slate-600 px-2 py-0.5 rounded-full">
            {orden.cantidadMesones} mesón{orden.cantidadMesones !== 1 ? 'es' : ''}
          </span>
          {fechaLabel && (
            <span className="text-xs text-slate-400">Aprobado: {fechaLabel}</span>
          )}
        </div>

        {orden.comentarios && (
          <p className="text-xs text-slate-500 mt-2 italic border-l-2 border-slate-200 pl-2">
            {orden.comentarios}
          </p>
        )}
      </div>

      {/* Expandir mesones */}
      <button
        onClick={() => setExpandido(!expandido)}
        className="w-full flex items-center justify-between px-4 py-2 text-xs text-slate-500 hover:bg-slate-50 border-t border-slate-100 transition"
      >
        <span>Ver mesones</span>
        {expandido ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
      </button>

      {expandido && (
        <div className="px-4 pb-3 space-y-1.5 border-t border-slate-100">
          {orden.mesones.map((m) => (
            <div key={m.id} className="flex items-center justify-between py-1">
              <div>
                <p className="text-xs font-medium text-slate-700">{m.nombreMaterial}</p>
                <p className="text-xs text-slate-400">{m.geometria}</p>
              </div>
              <span className="text-xs tabular-nums text-slate-600">{m.areaM2.toFixed(3)} m²</span>
            </div>
          ))}
        </div>
      )}

      {/* Acción */}
      {columna !== 'finalizado' && (
        <div className="px-4 pb-4 pt-2">
          <button
            onClick={handleAccion}
            disabled={loading}
            className={`w-full flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-semibold transition disabled:opacity-60 ${
              columna === 'porIniciar'
                ? 'bg-blue-600 hover:bg-blue-700 text-white'
                : 'bg-green-600 hover:bg-green-700 text-white'
            }`}
          >
            {columna === 'porIniciar' ? (
              <><PlayCircle size={16} /> {loading ? 'Iniciando...' : 'Iniciar producción'}</>
            ) : (
              <><CheckCircle2 size={16} /> {loading ? 'Finalizando...' : 'Marcar finalizado'}</>
            )}
          </button>
        </div>
      )}

      {columna === 'finalizado' && (
        <div className="px-4 pb-3 pt-1">
          <span className="flex items-center gap-1.5 text-xs text-green-600 font-medium">
            <CheckCircle2 size={13} /> Trabajo terminado
          </span>
        </div>
      )}
    </div>
  );
}
