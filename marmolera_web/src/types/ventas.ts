// ─── Enums ────────────────────────────────────────────────────────────────────

export type PlantillaGeometria = 'Rectangulo' | 'Forma_L' | 'Forma_U';

export type EstadoCotizacion =
  | 'Cotizado'
  | 'Aprobado'
  | 'EnProduccion'
  | 'Finalizado';

// ─── Cliente ──────────────────────────────────────────────────────────────────

export interface ClienteResponseDto {
  id: number;
  nombreCompleto: string;
  telefono: string;
  direccion: string;
  nit_Ci: string;
  fechaRegistro: string;
}

export interface ClienteCreateDto {
  nombreCompleto: string;
  telefono: string;
  direccion: string;
  nit_Ci?: string;
}

// ─── Detalle Cotización ───────────────────────────────────────────────────────

export interface DetalleCotizacionCreateDto {
  nombreMaterial: string;
  geometria: PlantillaGeometria;
  ladoA: number;
  ladoB: number;
  ladoC?: number;
  ancho?: number;
  precioPorM2: number;
}

export interface DetalleCotizacionResponseDto {
  id: number;
  nombreMaterial: string;
  geometria: string;
  medidasJson: string;
  precioPorM2: number;
  areaTotal: number;
  precioSubtotal: number;
}

// ─── Cotización ───────────────────────────────────────────────────────────────

export interface CotizacionCreateDto {
  clienteId: number;
  comentarios?: string;
  detalles: DetalleCotizacionCreateDto[];
}

export interface CotizacionResponseDto {
  id: number;
  comentarios?: string;
  precioTotal: number;
  estado: EstadoCotizacion;
  fechaCreacion: string;
  fechaAprobacion?: string;
  cliente: ClienteResponseDto;
  detalles: DetalleCotizacionResponseDto[];
}
