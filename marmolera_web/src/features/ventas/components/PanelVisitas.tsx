import { useEffect, useState } from 'react';
import { Calendar, MapPin, Phone, Clock, CheckCheck, AlertCircle } from 'lucide-react';
import { cotizacionesService } from '../services/cotizacionesService';
import type { VisitaResponseDto } from '../../../types/visitas';

interface Props {
  onConfirmada?: () => void;
}

function formatFecha(iso: string) {
  return new Date(iso).toLocaleString('es-BO', {
    weekday: 'short', day: '2-digit', month: 'short',
    hour: '2-digit', minute: '2-digit',
  });
}

export default function PanelVisitas({ onConfirmada }: Props) {
  const [visitas, setVisitas]                 = useState<VisitaResponseDto[]>([]);
  const [loading, setLoading]                 = useState(true);
  const [confirming, setConfirming]           = useState<number | null>(null);
  const [fechaNueva, setFechaNueva]           = useState<Record<number, string>>({});
  const [motivo, setMotivo]                   = useState<Record<number, string>>({});
  const [error, setError]                     = useState('');

  const cargar = async () => {
    try {
      setLoading(true);
      const data = await cotizacionesService.getVisitasPendientes();
      setVisitas(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { cargar(); }, []);

  const handleConfirmar = async (v: VisitaResponseDto) => {
    setError('');
    const fecha = fechaNueva[v.eventoId] || v.fechaVisita;
    try {
      await cotizacionesService.confirmarVisita(v.cotizacionId, {
        fechaConfirmada: new Date(fecha).toISOString(),
        motivo: motivo[v.eventoId] || null,
      });
      setConfirming(null);
      await cargar();
      onConfirmada?.();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al confirmar la visita.');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-40 text-slate-400 text-sm">
        Cargando visitas...
      </div>
    );
  }

  if (visitas.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-40 gap-2 text-slate-400">
        <Calendar size={32} className="opacity-20" />
        <p className="text-sm">No hay visitas de medición pendientes.</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {error && (
        <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-2.5 text-sm flex items-center gap-2">
          <AlertCircle size={15} /> {error}
        </div>
      )}

      {visitas.map((v) => (
        <div
          key={v.eventoId}
          className={`border rounded-xl p-4 space-y-3 transition ${
            v.estadoVisita === 'Confirmada'
              ? 'border-green-200 bg-green-50'
              : 'border-amber-200 bg-amber-50'
          }`}
        >
          {/* Cabecera */}
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="font-semibold text-slate-800">{v.clienteNombre}</p>
              <div className="flex items-center gap-3 mt-1 text-xs text-slate-500">
                <span className="flex items-center gap-1"><Phone size={11} /> {v.clienteTelefono}</span>
                <span className="flex items-center gap-1"><MapPin size={11} /> {v.clienteDireccion || 'Sin dirección'}</span>
              </div>
            </div>
            <span className={`shrink-0 text-xs font-medium px-2.5 py-1 rounded-full ${
              v.estadoVisita === 'Confirmada'
                ? 'bg-green-100 text-green-700'
                : 'bg-amber-100 text-amber-700'
            }`}>
              {v.estadoVisita}
            </span>
          </div>

          {/* Fecha */}
          <div className="flex items-center gap-2 text-sm">
            <Clock size={14} className="text-slate-400" />
            <span className="text-slate-700">
              {v.fueReprogramada && (
                <span className="text-slate-400 line-through mr-2 text-xs">
                  {formatFecha(v.fechaOriginal!)}
                </span>
              )}
              {formatFecha(v.fechaVisita)}
            </span>
          </div>

          {v.notas && (
            <p className="text-xs text-slate-500 bg-white/60 rounded-lg px-3 py-2">{v.notas}</p>
          )}

          {/* Acciones */}
          {v.estadoVisita === 'Pendiente' && (
            <>
              {confirming === v.eventoId ? (
                <div className="space-y-2 pt-1">
                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">Fecha confirmada</label>
                    <input
                      type="datetime-local"
                      defaultValue={v.fechaVisita.slice(0, 16)}
                      onChange={(e) => setFechaNueva((p) => ({ ...p, [v.eventoId]: e.target.value }))}
                      className="w-full px-3 py-2 text-sm border border-amber-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-400"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-slate-600 mb-1">Motivo de cambio (si aplica)</label>
                    <input
                      type="text"
                      placeholder="Ej. el cliente no podía ese día..."
                      onChange={(e) => setMotivo((p) => ({ ...p, [v.eventoId]: e.target.value }))}
                      className="w-full px-3 py-2 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-400"
                    />
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => setConfirming(null)}
                      className="flex-1 py-2 text-sm font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50"
                    >
                      Cancelar
                    </button>
                    <button
                      onClick={() => handleConfirmar(v)}
                      className="flex-1 flex items-center justify-center gap-1.5 py-2 text-sm font-semibold bg-green-600 text-white rounded-lg hover:bg-green-700 transition"
                    >
                      <CheckCheck size={14} /> Confirmar
                    </button>
                  </div>
                </div>
              ) : (
                <button
                  onClick={() => setConfirming(v.eventoId)}
                  className="w-full flex items-center justify-center gap-2 py-2 text-sm font-semibold bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition"
                >
                  <Calendar size={14} /> Confirmar / Reprogramar
                </button>
              )}
            </>
          )}

          {v.estadoVisita === 'Confirmada' && (
            <div className="flex items-center gap-2 text-sm text-green-700 font-medium">
              <CheckCheck size={15} /> Visita confirmada — en espera de realizarse
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
