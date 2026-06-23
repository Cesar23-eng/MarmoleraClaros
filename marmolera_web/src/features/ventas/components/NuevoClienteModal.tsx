import { useState } from 'react';
import { X } from 'lucide-react';
import { clientesService } from '../services/clientesService';
import type { ClienteCreateDto, ClienteResponseDto } from '../../../types/ventas';

interface Props {
  onClose: () => void;
  // Ahora devuelve el cliente completo, no solo el id
  onCreado: (cliente: ClienteResponseDto) => void;
}

export default function NuevoClienteModal({ onClose, onCreado }: Props) {
  const [form, setForm] = useState<ClienteCreateDto>({
    nombreCompleto: '',
    telefono: '',
    direccion: '',
    nit_Ci: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const set = (campo: keyof ClienteCreateDto, valor: string) =>
    setForm((p) => ({ ...p, [campo]: valor }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!form.nombreCompleto.trim()) { setError('El nombre es obligatorio.'); return; }
    if (!form.telefono.trim())       { setError('El teléfono es obligatorio.'); return; }
    try {
      setLoading(true);
      const nuevo = await clientesService.crear(form);
      // Pasar el cliente completo (con nombre real) al padre
      onCreado(nuevo);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setError(msg ?? 'Error al guardar el cliente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 px-4">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-xl">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 className="text-base font-bold text-slate-800">Nuevo Cliente</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600 transition">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-3 text-sm">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Nombre completo <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={form.nombreCompleto}
              onChange={(e) => set('nombreCompleto', e.target.value)}
              placeholder="Ej: Juan Pérez Mamani"
              required
              className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Teléfono <span className="text-red-500">*</span>
            </label>
            <input
              type="tel"
              value={form.telefono}
              onChange={(e) => set('telefono', e.target.value)}
              placeholder="Ej: 70012345"
              required
              className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Dirección</label>
            <input
              type="text"
              value={form.direccion}
              onChange={(e) => set('direccion', e.target.value)}
              placeholder="Ej: Av. Cristo Redentor #123"
              className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">NIT / CI</label>
            <input
              type="text"
              value={form.nit_Ci ?? ''}
              onChange={(e) => set('nit_Ci', e.target.value)}
              placeholder="Ej: 12345678"
              className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
            />
          </div>

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
              {loading ? 'Guardando...' : 'Crear cliente'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
