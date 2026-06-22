import { apiClient } from '../../../core/api/apiClient';
import type { NotificacionDto } from '../../../types/notificaciones';

const BASE = '/notificaciones';

export const notificacionesService = {
  getMias:          ()        => apiClient.get<NotificacionDto[]>(`${BASE}`).then(r => r.data),
  getCountNoLeidas: ()        => apiClient.get<{ count: number }>(`${BASE}/no-leidas/count`).then(r => r.data.count),
  marcarLeida:      (id: number) => apiClient.put(`${BASE}/${id}/leer`),
  marcarTodasLeidas:()        => apiClient.put(`${BASE}/leer-todas`),
  eliminar:         (id: number) => apiClient.delete(`${BASE}/${id}`),
};
