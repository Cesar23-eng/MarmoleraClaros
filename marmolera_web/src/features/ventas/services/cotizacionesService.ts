import { apiClient } from '../../../core/api/apiClient';
import type {
  CotizacionCreateDto,
  CotizacionResponseDto,
} from '../../../types/ventas';

const BASE = '/cotizaciones';

export const cotizacionesService = {
  /** Cotizaciones del usuario autenticado */
  getMias: () =>
    apiClient.get<CotizacionResponseDto[]>(BASE).then((r) => r.data),

  /** Todas las cotizaciones (Admin / Ventas) */
  getTodas: () =>
    apiClient.get<CotizacionResponseDto[]>(`${BASE}/todas`).then((r) => r.data),

  /** Por ID */
  getById: (id: number) =>
    apiClient.get<CotizacionResponseDto>(`${BASE}/${id}`).then((r) => r.data),

  /** Crear nueva cotización */
  crear: (dto: CotizacionCreateDto) =>
    apiClient.post<CotizacionResponseDto>(BASE, dto).then((r) => r.data),

  /** Editar cotización (solo estado "Cotizado") */
  editar: (id: number, dto: CotizacionCreateDto) =>
    apiClient.put<CotizacionResponseDto>(`${BASE}/${id}`, dto).then((r) => r.data),

  /** Aprobar → "Aprobado" */
  aprobar: (id: number) =>
    apiClient.put(`${BASE}/${id}/aprobar`).then((r) => r.data),

  /** Iniciar producción → "EnProduccion" */
  iniciarProduccion: (id: number) =>
    apiClient.put(`${BASE}/${id}/iniciar-produccion`).then((r) => r.data),

  /** Finalizar producción → "Finalizado" */
  finalizarProduccion: (id: number) =>
    apiClient.put(`${BASE}/${id}/finalizar-produccion`).then((r) => r.data),

  /** Pendientes para producción */
  getPendientesProduccion: () =>
    apiClient.get<CotizacionResponseDto[]>(`${BASE}/pendientes-produccion`).then((r) => r.data),
};
