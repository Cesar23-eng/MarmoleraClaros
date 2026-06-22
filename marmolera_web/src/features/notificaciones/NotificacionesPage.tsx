import { Bell } from 'lucide-react';

export default function NotificacionesPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Notificaciones</h1>
        <p className="text-slate-500 text-sm mt-1">Alertas del sistema por rol</p>
      </div>
      <div className="bg-white rounded-xl border border-slate-200 h-80 flex flex-col items-center justify-center gap-3 text-slate-400">
        <Bell size={40} className="opacity-20" />
        <p className="text-sm">Módulo de Notificaciones — próximamente</p>
      </div>
    </div>
  );
}
