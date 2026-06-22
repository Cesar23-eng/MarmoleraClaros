import { apiClient } from '../../../core/api/apiClient';
import type { ResumenGeneralDto, VentasMesDto, TopMaterialDto, EstadoDistribucionDto } from '../../../types/reportes';

const BASE = '/reportes';

export const reportesService = {
  getResumen:           ()             => apiClient.get<ResumenGeneralDto>(`${BASE}/resumen`).then(r => r.data),
  getVentasPorMes:      (meses = 12)   => apiClient.get<VentasMesDto[]>(`${BASE}/ventas-por-mes?meses=${meses}`).then(r => r.data),
  getTopMateriales:     (top = 5)      => apiClient.get<TopMaterialDto[]>(`${BASE}/top-materiales?top=${top}`).then(r => r.data),
  getCotizacionesPorEstado: ()         => apiClient.get<EstadoDistribucionDto[]>(`${BASE}/cotizaciones-por-estado`).then(r => r.data),
};
