import { Bell, CheckCheck, FileText, CheckCircle, PlayCircle, Package, Calendar, Info } from 'lucide-react';
import type { NotificacionDto, TipoNotificacion } from '../../../types/notificaciones';
import { useNotificaciones } from '../hooks/useNotificaciones';

// ─── Icono por tipo ───────────────────────────────────────────────────────────
const ICONO: Record<TipoNotificacion, React.ReactNode> = {
  NuevaCotizacion:    <FileText   size={16} className="text-blue-500" />,
  CotizacionAprobada: <CheckCircle size={16} className="text-green-500" />,
  OrdenIniciada:      <PlayCircle  size={16} className="text-orange-500" />,
  OrdenFinalizada:    <Package     size={16} className="text-purple-500" />,
  EventoProximo:      <Calendar    size={16} className="text-amber-500" />,
  General:            <Info        size={16} className="text-slate-400" />,
};

function tiempoRelativo(fecha: string): string {
  const diff = Date.now() - new Date(fecha).getTime();
  const min  = Math.floor(diff / 60000);
  if (min < 1)  return 'Ahora';
  if (min < 60) return `Hace ${min} min`;
  const h = Math.floor(min / 60);
  if (h < 24)   return `Hace ${h}h`;
  return `Hace ${Math.floor(h / 24)}d`;
}

// ─── Tarjeta individual ─────────────────────────────────────────────────────
function NotificacionItem({
  notif,
  onLeer,
}: {
  notif: NotificacionDto;
  onLeer: (id: number) => void;
}) {
  return (
    <div
      onClick={() => !notif.leida && onLeer(notif.id)}
      className={`flex gap-3 px-4 py-3 transition cursor-pointer ${
        notif.leida
          ? 'opacity-50 hover:opacity-70'
          : 'bg-blue-50/60 hover:bg-blue-50'
      }`}
    >
      {/* Punto no leída */}
      <div className="mt-1 shrink-0">
        {!notif.leida && (
          <span className="block w-2 h-2 rounded-full bg-blue-500 mt-0.5" />
        )}
        {notif.leida && <span className="block w-2 h-2" />}
      </div>

      {/* Icono tipo */}
      <div className="mt-0.5 shrink-0">
        {ICONO[notif.tipo] ?? <Info size={16} className="text-slate-400" />}
      </div>

      {/* Contenido */}
      <div className="flex-1 min-w-0">
        <p className="text-sm text-slate-700 leading-snug">{notif.mensaje}</p>
        <p className="text-xs text-slate-400 mt-0.5">{tiempoRelativo(notif.fechaCreacion)}</p>
      </div>
    </div>
  );
}

// ─── Panel principal (dropdown) ──────────────────────────────────────────
export default function NotificacionesPanel() {
  const { notificaciones, countNoLeidas, loading, marcarLeida, marcarTodasLeidas } = useNotificaciones();
  const [abierto, setAbierto] = useState(false);

  // Cerrar al click fuera
  const noLeidas = notificaciones.filter(n => !n.leida);
  const leidas   = notificaciones.filter(n =>  n.leida);

  return (
    <div className="relative">
      {/* Botón campana */}
      <button
        onClick={() => setAbierto(!abierto)}
        className="relative p-2 rounded-lg text-slate-500 hover:text-slate-700 hover:bg-slate-100 transition"
        aria-label="Notificaciones"
      >
        <Bell size={20} />
        {countNoLeidas > 0 && (
          <span className="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center px-1">
            {countNoLeidas > 99 ? '99+' : countNoLeidas}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {abierto && (
        <>
          {/* Overlay para cerrar */}
          <div className="fixed inset-0 z-40" onClick={() => setAbierto(false)} />

          <div className="absolute right-0 top-full mt-2 w-80 bg-white rounded-2xl shadow-xl border border-slate-200 z-50 overflow-hidden">
            {/* Header */}
            <div className="flex items-center justify-between px-4 py-3 border-b border-slate-100">
              <span className="text-sm font-semibold text-slate-800">
                Notificaciones {countNoLeidas > 0 && (
                  <span className="ml-1.5 text-xs bg-red-100 text-red-600 px-1.5 py-0.5 rounded-full font-bold">
                    {countNoLeidas}
                  </span>
                )}
              </span>
              {countNoLeidas > 0 && (
                <button
                  onClick={marcarTodasLeidas}
                  className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-800 transition"
                >
                  <CheckCheck size={13} /> Marcar todas
                </button>
              )}
            </div>

            {/* Lista */}
            <div className="max-h-96 overflow-y-auto divide-y divide-slate-100">
              {loading && (
                <div className="py-8 text-center text-sm text-slate-400">Cargando...</div>
              )}
              {!loading && notificaciones.length === 0 && (
                <div className="py-10 text-center">
                  <Bell size={28} className="mx-auto text-slate-200 mb-2" />
                  <p className="text-sm text-slate-400">Sin notificaciones</p>
                </div>
              )}
              {noLeidas.map(n => (
                <NotificacionItem key={n.id} notif={n} onLeer={marcarLeida} />
              ))}
              {leidas.length > 0 && noLeidas.length > 0 && (
                <div className="px-4 py-1.5 text-xs text-slate-400 uppercase tracking-wide bg-slate-50">
                  Anteriores
                </div>
              )}
              {leidas.map(n => (
                <NotificacionItem key={n.id} notif={n} onLeer={marcarLeida} />
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

// Exportamos el hook también para usarlo en el navbar
export { useNotificaciones };
