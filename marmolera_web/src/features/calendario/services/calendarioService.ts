import { apiClient } from '../../../core/api/apiClient';
import type { EventoCalendarioDto, CrearEventoDto, ReprogramarEventoDto } from '../../../types/calendario';

const BASE = '/calendario';

export const calendarioService = {
  getPorMes: (anio: number, mes: number) =>
    apiClient.get<EventoCalendarioDto[]>(`${BASE}?anio=${anio}&mes=${mes}`).then(r => r.data),

  getById: (id: number) =>
    apiClient.get<EventoCalendarioDto>(`${BASE}/${id}`).then(r => r.data),

  crear: (dto: CrearEventoDto) =>
    apiClient.post<EventoCalendarioDto>(BASE, dto).then(r => r.data),

  actualizar: (id: number, dto: CrearEventoDto) =>
    apiClient.put<EventoCalendarioDto>(`${BASE}/${id}`, dto).then(r => r.data),

  reprogramar: (id: number, dto: ReprogramarEventoDto) =>
    apiClient.put<EventoCalendarioDto>(`${BASE}/${id}/reprogramar`, dto).then(r => r.data),

  eliminar: (id: number) =>
    apiClient.delete(`${BASE}/${id}`),
};
