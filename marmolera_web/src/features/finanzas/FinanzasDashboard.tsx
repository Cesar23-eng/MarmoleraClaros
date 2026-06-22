import { DollarSign } from 'lucide-react';

export default function FinanzasDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Finanzas</h1>
        <p className="text-slate-500 text-sm mt-1">Contabilidad y estado financiero</p>
      </div>
      <div className="bg-white rounded-xl border border-slate-200 h-80 flex flex-col items-center justify-center gap-3 text-slate-400">
        <DollarSign size={40} className="opacity-20" />
        <p className="text-sm">Módulo de Finanzas — próximamente</p>
      </div>
    </div>
  );
}
