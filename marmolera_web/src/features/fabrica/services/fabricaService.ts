import { apiClient } from '../../../core/api/apiClient';
import type { CotizacionKanbanDto, AsignarOperarioDto, AgregarNotaDto } from '../../../types/fabrica';

const BASE = '/fabrica';

export const fabricaService = {
  getPorIniciar:    ()                          => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/por-iniciar`).then(r => r.data),
  getEnProduccion:  ()                          => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/en-produccion`).then(r => r.data),
  getFinalizados:   ()                          => apiClient.get<CotizacionKanbanDto[]>(`${BASE}/finalizados`).then(r => r.data),
  iniciarOrden:     (id: number)                => apiClient.post(`${BASE}/${id}/iniciar`),
  finalizarOrden:   (id: number)                => apiClient.post(`${BASE}/${id}/finalizar`),
  asignarOperario:  (id: number, dto: AsignarOperarioDto) => apiClient.put(`${BASE}/${id}/asignar-operario`, dto),
  agregarNota:      (id: number, dto: AgregarNotaDto)     => apiClient.put(`${BASE}/${id}/nota`, dto),
};
