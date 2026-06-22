import type { CotizacionKanbanDto } from '../../../types/fabrica';
import KanbanCard from './KanbanCard';
import { Inbox } from 'lucide-react';

interface Props {
  titulo: string;
  color: 'blue' | 'orange' | 'green';
  ordenes: CotizacionKanbanDto[];
  columna: 'porIniciar' | 'enProduccion' | 'finalizado';
  onAccion: (id: number) => Promise<void>;
}

const headerColor = {
  blue:   'bg-blue-50 border-blue-200 text-blue-700',
  orange: 'bg-orange-50 border-orange-200 text-orange-700',
  green:  'bg-green-50 border-green-200 text-green-700',
};

const dotColor = {
  blue:   'bg-blue-500',
  orange: 'bg-orange-500',
  green:  'bg-green-500',
};

export default function KanbanColumn({ titulo, color, ordenes, columna, onAccion }: Props) {
  return (
    <div className="flex flex-col gap-3 min-w-0">
      {/* Header columna */}
      <div className={`flex items-center justify-between px-4 py-2.5 rounded-xl border ${headerColor[color]}`}>
        <div className="flex items-center gap-2">
          <span className={`w-2 h-2 rounded-full ${dotColor[color]}`} />
          <span className="text-sm font-semibold">{titulo}</span>
        </div>
        <span className="text-sm font-bold">{ordenes.length}</span>
      </div>

      {/* Tarjetas */}
      <div className="space-y-3">
        {ordenes.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-10 gap-2 text-slate-400">
            <Inbox size={28} className="opacity-30" />
            <p className="text-xs">Sin órdenes</p>
          </div>
        ) : (
          ordenes.map((o) => (
            <KanbanCard
              key={o.id}
              orden={o}
              columna={columna}
              onAccion={onAccion}
            />
          ))
        )}
      </div>
    </div>
  );
}
