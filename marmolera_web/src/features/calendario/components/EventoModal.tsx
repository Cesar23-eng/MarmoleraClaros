import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import type { EventoCalendarioDto, CrearEventoDto, TipoEvento } from '../../../types/calendario';

const TIPOS: TipoEvento[] = ['Entrega', 'Instalacion', 'Medicion', 'Reunion', 'Otro'];

const COLOR_POR_TIPO: Record<TipoEvento, string> = {
  Entrega:    '#3B82F6',
  Instalacion:'#8B5CF6',
  Medicion:   '#F59E0B',
  Reunion:    '#10B981',
  Otro:       '#6B7280',
};

interface Props {
  evento?: EventoCalendarioDto | null;
  fechaInicial?: string;
  onGuardar: (dto: CrearEventoDto) => Promise<void>;
  onCerrar: () => void;
}

export default function EventoModal({ evento, fechaInicial, onGuardar, onCerrar }: Props) {
  const hoy = fechaInicial ?? new Date().toISOString().slice(0, 16);

  const [titulo,      setTitulo]      = useState(evento?.titulo ?? '');
  const [tipo,        setTipo]        = useState<TipoEvento>(evento?.tipo ?? 'Entrega');
  const [fechaInicio, setFechaInicio] = useState(evento ? evento.fechaInicio.slice(0, 16) : hoy);
  const [fechaFin,    setFechaFin]    = useState(evento ? evento.fechaFin.slice(0, 16) : hoy);
  const [notas,       setNotas]       = useState(evento?.notas ?? '');
  const [loading,     setLoading]     = useState(false);

  // Al cambiar tipo actualizar color automáticamente
  const colorActual = evento?.color ?? COLOR_POR_TIPO[tipo];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) return;
    try {
      setLoading(true);
      await onGuardar({
        titulo:      titulo.trim(),
        tipo,
        fechaInicio: new Date(fechaInicio).toISOString(),
        fechaFin:    new Date(fechaFin).toISOString(),
        color:       colorActual,
        notas:       notas.trim() || undefined,
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 className="text-lg font-semibold text-slate-800">
            {evento ? 'Editar evento' : 'Nuevo evento'}
          </h2>
          <button onClick={onCerrar} className="text-slate-400 hover:text-slate-600 transition">
            <X size={20} />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {/* Título */}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Título *</label>
            <input
              type="text"
              value={titulo}
              onChange={e => setTitulo(e.target.value)}
              placeholder="Ej: Entrega mesón Sr. Pérez"
              required
              className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Tipo */}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Tipo</label>
            <div className="flex flex-wrap gap-2">
              {TIPOS.map(t => (
                <button
                  key={t} type="button"
                  onClick={() => setTipo(t)}
                  style={{ borderColor: tipo === t ? COLOR_POR_TIPO[t] : undefined,
                           backgroundColor: tipo === t ? COLOR_POR_TIPO[t] + '20' : undefined }}
                  className={`px-3 py-1 text-xs font-medium rounded-full border transition ${
                    tipo === t ? 'text-slate-800 border-2' : 'border-slate-200 text-slate-500 hover:border-slate-300'
                  }`}
                >
                  {t}
                </button>
              ))}
            </div>
          </div>

          {/* Fechas */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Inicio</label>
              <input
                type="datetime-local"
                value={fechaInicio}
                onChange={e => setFechaInicio(e.target.value)}
                required
                className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Fin</label>
              <input
                type="datetime-local"
                value={fechaFin}
                onChange={e => setFechaFin(e.target.value)}
                required
                className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* Notas */}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Notas</label>
            <textarea
              value={notas}
              onChange={e => setNotas(e.target.value)}
              rows={2}
              placeholder="Observaciones opcionales..."
              className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
            />
          </div>

          {/* Acciones */}
          <div className="flex gap-3 pt-2">
            <button
              type="button" onClick={onCerrar}
              className="flex-1 border border-slate-300 text-slate-600 py-2 rounded-lg text-sm font-medium hover:bg-slate-50 transition"
            >
              Cancelar
            </button>
            <button
              type="submit" disabled={loading}
              className="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg text-sm font-semibold transition disabled:opacity-60"
            >
              {loading ? 'Guardando...' : evento ? 'Actualizar' : 'Crear evento'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
