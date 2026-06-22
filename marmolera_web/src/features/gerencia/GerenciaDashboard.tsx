import { BarChart2 } from 'lucide-react';

export default function GerenciaDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Gerencia</h1>
        <p className="text-slate-500 text-sm mt-1">Vista ejecutiva del negocio</p>
      </div>
      <div className="bg-white rounded-xl border border-slate-200 h-80 flex flex-col items-center justify-center gap-3 text-slate-400">
        <BarChart2 size={40} className="opacity-20" />
        <p className="text-sm">Dashboard de Gerencia — próximamente</p>
      </div>
    </div>
  );
}
