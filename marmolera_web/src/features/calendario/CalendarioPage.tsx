import { Calendar } from 'lucide-react';

export default function CalendarioPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Calendario</h1>
        <p className="text-slate-500 text-sm mt-1">Eventos de entregas y producción</p>
      </div>
      <div className="bg-white rounded-xl border border-slate-200 h-80 flex flex-col items-center justify-center gap-3 text-slate-400">
        <Calendar size={40} className="opacity-20" />
        <p className="text-sm">Módulo de Calendario — próximamente</p>
      </div>
    </div>
  );
}
