import { apiClient } from '../../../core/api/apiClient';
import type { ClienteCreateDto, ClienteResponseDto } from '../../../types/ventas';

const BASE = '/clientes';

export const clientesService = {
  getAll: () =>
    apiClient.get<ClienteResponseDto[]>(BASE).then((r) => r.data),

  getById: (id: number) =>
    apiClient.get<ClienteResponseDto>(`${BASE}/${id}`).then((r) => r.data),

  crear: (dto: ClienteCreateDto) =>
    apiClient.post<ClienteResponseDto>(BASE, dto).then((r) => r.data),

  editar: (id: number, dto: ClienteCreateDto) =>
    apiClient.put<ClienteResponseDto>(`${BASE}/${id}`, dto).then((r) => r.data),
};
