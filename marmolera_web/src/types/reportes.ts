export interface ResumenGeneralDto {
  totalCotizaciones: number;
  cotizacionesAprobadas: number;
  cotizacionesEnProduccion: number;
  cotizacionesFinalizadas: number;
  ingresosTotales: number;
  ingresosEsteMes: number;
  totalClientes: number;
  clientesNuevosEsteMes: number;
}

export interface VentasMesDto {
  anio: number;
  mes: number;
  mesNombre: string;
  total: number;
  cantidad: number;
}

export interface TopMaterialDto {
  nombreMaterial: string;
  vecesUsado: number;
  areaTotalM2: number;
  ingresoGenerado: number;
}

export interface EstadoDistribucionDto {
  estado: string;
  cantidad: number;
  porcentaje: number;
}
