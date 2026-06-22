import { useEffect, useRef, useState } from 'react';
import { Bell, CheckCheck, X } from 'lucide-react';
import type { NotificacionDto } from '../../types/notificaciones';
import { ICONO_TIPO } from '../../types/notificaciones';
import { notificacionesService } from './services/notificacionesService';

// Intervalo de polling (ms). En producción se puede reemplazar por WebSocket/SSE.
const POLLING_MS = 30_000;

function tiempoRelativo(fecha: string): string {
  const diff = Math.floor((Date.now() - new Date(fecha).getTime()) / 1000);
  if (diff < 60)   return 'Hace un momento';
  if (diff < 3600) return `Hace ${Math.floor(diff / 60)} min`;
  if (diff < 86400) return `Hace ${Math.floor(diff / 3600)} h`;
  return new Date(fecha).toLocaleDateString('es-BO', { day: '2-digit', month: 'short' });
}

export default function CampanaNotificaciones() {
  const [notifs,   setNotifs]   = useState<NotificacionDto[]>([]);
  const [conteo,   setConteo]   = useState(0);
  const [abierto,  setAbierto]  = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  // ── Cargar conteo (poll silencioso) ──────────────────────────────────────
  const cargarConteo = async () => {
    try { setConteo(await notificacionesService.conteo()); } catch { /* silencioso */ }
  };

  // ── Abrir dropdown y cargar lista completa ───────────────────────────────
  const abrirDropdown = async () => {
    setAbierto(true);
    try { setNotifs(await notificacionesService.listar()); } catch { /* silencioso */ }
  };

  // ── Marcar una como leída ─────────────────────────────────────────────────
  const marcarLeida = async (id: number) => {
    await notificacionesService.marcarLeida(id);
    setNotifs(prev => prev.map(n => n.id === id ? { ...n, leida: true } : n));
    setConteo(prev => Math.max(0, prev - 1));
  };

  // ── Marcar todas como leídas ──────────────────────────────────────────────
  const marcarTodas = async () => {
    await notificacionesService.marcarTodas();
    setNotifs(prev => prev.map(n => ({ ...n, leida: true })));
    setConteo(0);
  };

  // ── Cerrar al hacer click fuera ───────────────────────────────────────────
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node))
        setAbierto(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  // ── Polling inicial + intervalo ───────────────────────────────────────────
  useEffect(() => {
    cargarConteo();
    const id = setInterval(cargarConteo, POLLING_MS);
    return () => clearInterval(id);
  }, []);

  return (
    <div ref={ref} className="relative">
      {/* Botón campana */}
      <button
        onClick={() => abierto ? setAbierto(false) : abrirDropdown()}
        className="relative p-2 text-slate-500 hover:text-slate-800 hover:bg-slate-100 rounded-lg transition"
        aria-label="Notificaciones"
      >
        <Bell size={20} />
        {conteo > 0 && (
          <span className="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center leading-none">
            {conteo > 9 ? '9+' : conteo}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {abierto && (
        <div className="absolute right-0 top-full mt-2 w-96 bg-white border border-slate-200 rounded-2xl shadow-xl z-50 overflow-hidden">
          {/* Header dropdown */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-slate-100">
            <h3 className="font-semibold text-slate-800 text-sm">Notificaciones</h3>
            <div className="flex items-center gap-1">
              {conteo > 0 && (
                <button
                  onClick={marcarTodas}
                  className="flex items-center gap-1 text-xs text-slate-400 hover:text-blue-600 px-2 py-1 rounded-lg hover:bg-blue-50 transition"
                  title="Marcar todas como leídas"
                >
                  <CheckCheck size={12} /> Leer todas
                </button>
              )}
              <button onClick={() => setAbierto(false)} className="p-1 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition">
                <X size={14} />
              </button>
            </div>
          </div>

          {/* Lista */}
          <div className="max-h-[420px] overflow-y-auto divide-y divide-slate-50">
            {notifs.length === 0 ? (
              <div className="py-12 text-center text-slate-400 text-sm">
                <Bell size={28} className="mx-auto mb-2 opacity-30" />
                Sin notificaciones
              </div>
            ) : (
              notifs.map(n => (
                <div
                  key={n.id}
                  onClick={() => !n.leida && marcarLeida(n.id)}
                  className={`flex gap-3 px-4 py-3 cursor-pointer transition ${
                    n.leida
                      ? 'bg-white hover:bg-slate-50'
                      : 'bg-blue-50/60 hover:bg-blue-50'
                  }`}
                >
                  {/* Icono tipo */}
                  <span className="text-lg mt-0.5 shrink-0">{ICONO_TIPO[n.tipo]}</span>

                  {/* Contenido */}
                  <div className="flex-1 min-w-0">
                    <p className={`text-sm leading-snug ${
                      n.leida ? 'text-slate-600' : 'text-slate-800 font-medium'
                    }`}>
                      {n.titulo}
                    </p>
                    <p className="text-xs text-slate-500 mt-0.5 leading-relaxed line-clamp-2">{n.mensaje}</p>
                    <p className="text-xs text-slate-400 mt-1">{tiempoRelativo(n.fechaCreacion)}</p>
                  </div>

                  {/* Punto no leída */}
                  {!n.leida && (
                    <span className="w-2 h-2 bg-blue-500 rounded-full shrink-0 mt-1.5" />
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
