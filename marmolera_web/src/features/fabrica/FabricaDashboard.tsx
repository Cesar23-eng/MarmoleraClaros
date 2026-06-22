import { Hammer } from 'lucide-react';

export default function FabricaDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Control de Producción</h1>
        <p className="text-slate-500 text-sm mt-1">Gestión de órdenes y operarios</p>
      </div>
      <div className="bg-white rounded-xl border border-slate-200 h-80 flex flex-col items-center justify-center gap-3 text-slate-400">
        <Hammer size={40} className="opacity-20" />
        <p className="text-sm">Módulo de Fábrica — próximamente</p>
      </div>
    </div>
  );
}
