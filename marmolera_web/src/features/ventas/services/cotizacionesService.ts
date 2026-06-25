import { apiClient } from '../../../core/api/apiClient';
import type { CotizacionResponseDto } from '../../../types/ventas';

const BASE = '/cotizaciones';

export const cotizacionesService = {
  getMias: () =>
    apiClient.get<CotizacionResponseDto[]>(BASE).then((r) => r.data),

  getTodas: () =>
    apiClient.get<CotizacionResponseDto[]>(`${BASE}/todas`).then((r) => r.data),

  getById: (id: number) =>
    apiClient.get<CotizacionResponseDto>(`${BASE}/${id}`).then((r) => r.data),

  crearRaw: (payload: object) =>
    apiClient.post<CotizacionResponseDto>(BASE, payload).then((r) => r.data),

  editarRaw: (id: number, payload: object) =>
    apiClient.put<CotizacionResponseDto>(`${BASE}/${id}`, payload).then((r) => r.data),

  eliminar: (id: number) =>
    apiClient.delete(`${BASE}/${id}`).then((r) => r.data),

  aprobar: (id: number) =>
    apiClient.put(`${BASE}/${id}/aprobar`).then((r) => r.data),

  iniciarProduccion: (id: number) =>
    apiClient.put(`${BASE}/${id}/iniciar-produccion`).then((r) => r.data),

  finalizarProduccion: (id: number) =>
    apiClient.put(`${BASE}/${id}/finalizar-produccion`).then((r) => r.data),

  getPendientesProduccion: () =>
    apiClient.get<CotizacionResponseDto[]>(`${BASE}/pendientes-produccion`).then((r) => r.data),
};
