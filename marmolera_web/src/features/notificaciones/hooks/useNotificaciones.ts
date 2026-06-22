import { useState, useEffect, useCallback } from 'react';
import type { NotificacionDto } from '../../../types/notificaciones';
import { notificacionesService } from '../services/notificacionesService';

export function useNotificaciones() {
  const [notificaciones, setNotificaciones] = useState<NotificacionDto[]>([]);
  const [countNoLeidas,  setCountNoLeidas]  = useState(0);
  const [loading,        setLoading]        = useState(true);

  const cargar = useCallback(async () => {
    try {
      const [lista, count] = await Promise.all([
        notificacionesService.getMias(),
        notificacionesService.getCountNoLeidas(),
      ]);
      setNotificaciones(lista);
      setCountNoLeidas(count);
    } catch { /* silencioso */ }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  // Polling cada 60 segundos
  useEffect(() => {
    const interval = setInterval(cargar, 60_000);
    return () => clearInterval(interval);
  }, [cargar]);

  const marcarLeida = async (id: number) => {
    await notificacionesService.marcarLeida(id);
    setNotificaciones(prev =>
      prev.map(n => n.id === id ? { ...n, leida: true } : n)
    );
    setCountNoLeidas(c => Math.max(0, c - 1));
  };

  const marcarTodasLeidas = async () => {
    await notificacionesService.marcarTodasLeidas();
    setNotificaciones(prev => prev.map(n => ({ ...n, leida: true })));
    setCountNoLeidas(0);
  };

  return { notificaciones, countNoLeidas, loading, cargar, marcarLeida, marcarTodasLeidas };
}
