import { Clock, User, Package, Play, CheckCircle } from 'lucide-react';
import type { CotizacionKanbanDto } from '../../../types/fabrica';

const formatBs = (n: number) =>
  new Intl.NumberFormat('es-BO', { style: 'currency', currency: 'BOB', minimumFractionDigits: 0 }).format(n);

const formatFecha = (iso: string | null) => {
  if (!iso) return null;
  return new Date(iso).toLocaleDateString('es-BO', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
};

const COLUMN_STYLES = {
  blue:   { header: 'bg-blue-50 border-blue-200',   badge: 'bg-blue-100 text-blue-700',   btn: 'bg-blue-600 hover:bg-blue-700' },
  orange: { header: 'bg-orange-50 border-orange-200', badge: 'bg-orange-100 text-orange-700', btn: 'bg-orange-600 hover:bg-orange-700' },
  green:  { header: 'bg-green-50 border-green-200',  badge: 'bg-green-100 text-green-700',  btn: '' },
} as const;

type ColColor = keyof typeof COLUMN_STYLES;
type Columna  = 'porIniciar' | 'enProduccion' | 'finalizado';

interface Props {
  titulo:   string;
  color:    ColColor;
  ordenes:  CotizacionKanbanDto[];
  columna:  Columna;
  onAccion: (id: number) => Promise<void>;
}

function KanbanCard({ orden, columna, color, onAccion }: {
  orden: CotizacionKanbanDto;
  columna: Columna;
  color: ColColor;
  onAccion: (id: number) => Promise<void>;
}) {
  const styles = COLUMN_STYLES[color];

  return (
    <div className="bg-white rounded-xl border border-slate-200 p-4 shadow-sm space-y-3 hover:shadow-md transition">
      {/* Cliente + precio */}
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="font-semibold text-slate-800 text-sm leading-tight">{orden.nombreCliente}</p>
          {orden.telefono && <p className="text-xs text-slate-400">{orden.telefono}</p>}
        </div>
        <span className="text-xs font-bold text-slate-700 tabular-nums whitespace-nowrap">{formatBs(orden.precioTotal)}</span>
      </div>

      {/* Meta */}
      <div className="flex flex-wrap gap-2 text-xs text-slate-500">
        <span className="flex items-center gap-1">
          <Package size={11} /> {orden.totalPiezas} {orden.totalPiezas === 1 ? 'pieza' : 'piezas'}
        </span>
        <span className="flex items-center gap-1">
          <Clock size={11} /> #{orden.cotizacionId}
        </span>
        {orden.operarioNombre && (
          <span className="flex items-center gap-1">
            <User size={11} /> {orden.operarioNombre}
          </span>
        )}
      </div>

      {/* Fechas */}
      {orden.fechaInicio && (
        <p className="text-xs text-slate-400">Inicio: {formatFecha(orden.fechaInicio)}</p>
      )}
      {orden.fechaFin && (
        <p className="text-xs text-green-600 font-medium">Fin: {formatFecha(orden.fechaFin)}</p>
      )}

      {/* Nota */}
      {orden.notas && (
        <p className="text-xs text-slate-500 italic border-t border-slate-100 pt-2">{orden.notas}</p>
      )}

      {/* Botón de acción */}
      {columna === 'porIniciar' && (
        <button
          onClick={() => onAccion(orden.ordenFabricaId)}
          className={`w-full flex items-center justify-center gap-2 text-xs text-white font-medium py-2 rounded-lg transition ${styles.btn}`}
        >
          <Play size={12} /> Iniciar producción
        </button>
      )}
      {columna === 'enProduccion' && (
        <button
          onClick={() => onAccion(orden.ordenFabricaId)}
          className={`w-full flex items-center justify-center gap-2 text-xs text-white font-medium py-2 rounded-lg transition ${styles.btn}`}
        >
          <CheckCircle size={12} /> Marcar finalizado
        </button>
      )}
    </div>
  );
}

export default function KanbanColumn({ titulo, color, ordenes, columna, onAccion }: Props) {
  const styles = COLUMN_STYLES[color];

  return (
    <div className="flex flex-col gap-3">
      {/* Header columna */}
      <div className={`flex items-center justify-between px-4 py-2.5 rounded-xl border ${styles.header}`}>
        <span className="font-semibold text-slate-700 text-sm">{titulo}</span>
        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${styles.badge}`}>
          {ordenes.length}
        </span>
      </div>

      {/* Tarjetas */}
      {ordenes.length === 0 ? (
        <div className="text-center py-10 text-slate-300 text-sm border-2 border-dashed border-slate-100 rounded-xl">
          Sin órdenes
        </div>
      ) : (
        ordenes.map(o => (
          <KanbanCard key={o.ordenFabricaId} orden={o} columna={columna} color={color} onAccion={onAccion} />
        ))
      )}
    </div>
  );
}
