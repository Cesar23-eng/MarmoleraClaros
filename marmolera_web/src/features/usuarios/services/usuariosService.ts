import { apiClient } from '../../../core/api/apiClient';
import type { UsuarioDto, CrearUsuarioDto, EditarUsuarioDto, CambiarPasswordDto } from '../../../types/usuarios';

const BASE = '/usuarios';

export const usuariosService = {
  listar:           ()                              => apiClient.get<UsuarioDto[]>(BASE).then(r => r.data),
  obtener:          (id: string)                   => apiClient.get<UsuarioDto>(`${BASE}/${id}`).then(r => r.data),
  crear:            (dto: CrearUsuarioDto)          => apiClient.post<UsuarioDto>(BASE, dto).then(r => r.data),
  editar:           (id: string, dto: EditarUsuarioDto) => apiClient.put<UsuarioDto>(`${BASE}/${id}`, dto).then(r => r.data),
  cambiarPassword:  (id: string, dto: CambiarPasswordDto) => apiClient.put(`${BASE}/${id}/password`, dto),
  desactivar:       (id: string)                   => apiClient.delete(`${BASE}/${id}`),
};
