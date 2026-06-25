import { useState } from 'react';
import { X, CheckCircle, Calendar, Ruler } from 'lucide-react';
import { cotizacionesService } from '../services/cotizacionesService';
import type { CotizacionResponseDto } from '../../../types/ventas';

interface Props {
  cotizacion: CotizacionResponseDto;
  onClose: () => void;
  onAprobada: () => void;
}

export default function AprobarCotizacionModal({ cotizacion, onClose, onAprobada }: Props) {
  const [requiereMedicion, setRequiereMedicion] = useState(false);
  const [fechaVisita, setFechaVisita]           = useState('');
  const [notasVisita, setNotasVisita]           = useState('');
  const [loading, setLoading]                   = useState(false);
  const [error, setError]                       = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (requiereMedicion && !fechaVisita) {
      setError('Debes indicar una fecha propuesta para la visita.');
      return;
    }

    try {
      setLoading(true);
      await cotizacionesService.aprobar(cotizacion.id, {
        requiereMedicion,
        fechaVisita: requiereMedicion ? new Date(fechaVisita).toISOString() : null,
        notasVisita: notasVisita || null,
      });
      onAprobada();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al aprobar la cotización.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <div className="flex items-center gap-2">
            <CheckCircle size={20} className="text-green-600" />
            <h2 className="text-base font-bold text-slate-800">Aprobar Cotización #{cotizacion.id}</h2>
          </div>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* Resumen */}
          <div className="bg-slate-50 rounded-xl px-4 py-3 text-sm">
            <p className="text-slate-500">Cliente</p>
            <p className="font-semibold text-slate-800">{cotizacion.cliente.nombreCompleto}</p>
            <p className="text-slate-500 mt-1">Total: <span className="font-bold text-slate-800">Bs {cotizacion.precioTotal.toFixed(2)}</span></p>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-2.5 text-sm">{error}</div>
          )}

          {/* ¿Requiere medición? */}
          <div className="space-y-3">
            <p className="text-sm font-semibold text-slate-700">¿Requiere visita de medición?</p>
            <div className="grid grid-cols-2 gap-3">
              <button
                type="button"
                onClick={() => setRequiereMedicion(false)}
                className={`flex items-center justify-center gap-2 py-3 rounded-xl border-2 text-sm font-medium transition ${
                  !requiereMedicion
                    ? 'border-green-500 bg-green-50 text-green-700'
                    : 'border-slate-200 text-slate-500 hover:border-slate-300'
                }`}
              >
                <CheckCircle size={16} /> No, pasar a fábrica
              </button>
              <button
                type="button"
                onClick={() => setRequiereMedicion(true)}
                className={`flex items-center justify-center gap-2 py-3 rounded-xl border-2 text-sm font-medium transition ${
                  requiereMedicion
                    ? 'border-amber-500 bg-amber-50 text-amber-700'
                    : 'border-slate-200 text-slate-500 hover:border-slate-300'
                }`}
              >
                <Ruler size={16} /> Sí, agendar visita
              </button>
            </div>
          </div>

          {/* Datos visita */}
          {requiereMedicion && (
            <div className="space-y-3 p-4 bg-amber-50 border border-amber-100 rounded-xl">
              <div className="flex items-center gap-2 text-amber-700 text-sm font-semibold">
                <Calendar size={15} /> Datos de la visita
              </div>
              <div>
                <label className="block text-xs font-medium text-slate-600 mb-1">Fecha propuesta *</label>
                <input
                  type="datetime-local"
                  value={fechaVisita}
                  onChange={(e) => setFechaVisita(e.target.value)}
                  min={new Date().toISOString().slice(0, 16)}
                  className="w-full px-3 py-2 text-sm border border-amber-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-400"
                  required={requiereMedicion}
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-slate-600 mb-1">Notas para la arquitecta</label>
                <textarea
                  value={notasVisita}
                  onChange={(e) => setNotasVisita(e.target.value)}
                  rows={2}
                  placeholder="Dirección de acceso, referencia, qué medir..."
                  className="w-full px-3 py-2 text-sm border border-amber-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-400 resize-none"
                />
              </div>
            </div>
          )}

          {/* Acciones */}
          <div className="flex justify-end gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-800"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex items-center gap-2 px-5 py-2 text-sm font-semibold bg-green-600 text-white rounded-lg hover:bg-green-700 transition disabled:opacity-60"
            >
              <CheckCircle size={15} />
              {loading ? 'Aprobando...' : 'Confirmar aprobación'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
