import { X, CheckCircle } from 'lucide-react';
import type { CotizacionResponseDto } from '../../../types/ventas';
import EstadoBadge from './EstadoBadge';
import { cotizacionesService } from '../services/cotizacionesService';
import { useAuthStore } from '../../../core/store/useAuthStore';
import { useState } from 'react';

interface Props {
  cotizacion: CotizacionResponseDto;
  onClose: () => void;
  onActualizada: () => void;
}

export default function DetalleCotizacionModal({ cotizacion, onClose, onActualizada }: Props) {
  const { role } = useAuthStore();
  const [loading, setLoading] = useState(false);

  const puedeAprobar =
    cotizacion.estado === 'Cotizado' && (role === 'Admin' || role === 'Ventas');

  const handleAprobar = async () => {
    try {
      setLoading(true);
      await cotizacionesService.aprobar(cotizacion.id);
      onActualizada();
    } finally {
      setLoading(false);
    }
  };

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

          {/* Tabla de mesones */}
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
          <div className="flex justify-end gap-3">
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
                {loading ? 'Aprobando...' : 'Aprobar Cotización'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
