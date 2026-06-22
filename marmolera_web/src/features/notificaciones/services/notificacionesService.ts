import { apiClient } from '../../../core/api/apiClient';
import type { NotificacionDto } from '../../../types/notificaciones';

export const notificacionesService = {
  listar:          (soloNoLeidas = false) =>
    apiClient.get<NotificacionDto[]>('/notificaciones', { params: { soloNoLeidas } }).then(r => r.data),

  conteo:          () =>
    apiClient.get<{ total: number }>('/notificaciones/conteo').then(r => r.data.total),

  marcarLeida:     (id: number) =>
    apiClient.put(`/notificaciones/${id}/leer`),

  marcarTodas:     () =>
    apiClient.put('/notificaciones/leer-todas'),
};
