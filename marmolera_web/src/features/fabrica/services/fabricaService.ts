import { apiClient } from '../../../core/api/apiClient';
import type { CotizacionKanbanDto } from '../../../types/fabrica';

const BASE = '/fabrica';

export const fabricaService = {
  getPorIniciar:   () => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/por-iniciar`).then(r => r.data),
  getEnProduccion: () => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/en-produccion`).then(r => r.data),
  getFinalizados:  () => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/finalizados`).then(r => r.data),
  iniciarOrden:    (id: number) => apiClient.put(`${BASE}/${id}/iniciar`).then(r => r.data),
  finalizarOrden:  (id: number) => apiClient.put(`${BASE}/${id}/finalizar`).then(r => r.data),
};
