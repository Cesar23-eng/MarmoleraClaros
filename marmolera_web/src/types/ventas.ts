// ─── Enums ────────────────────────────────────────────────────────────────────

export type EstadoCotizacion =
  | 'Cotizado'
  | 'PendienteVisita'
  | 'Aprobado'
  | 'EnProduccion'
  | 'Finalizado';

// ─── Cliente ──────────────────────────────────────────────────────────────────

export interface ClienteResponseDto {
  id:             number;
  nombreCompleto: string;
  telefono:       string;
  direccion:      string;
  nit_Ci:         string;
  fechaRegistro:  string;
}

export interface ClienteCreateDto {
  nombreCompleto: string;
  telefono:       string;
  direccion?:     string;
  referencia?:    string;
  nit_Ci?:        string;
}

// ─── Detalle cotización ───────────────────────────────────────────────────────

export type Geometria = 'Rectangulo' | 'Forma_L' | 'Forma_U';

export interface DetalleCotizacionCreateDto {
  nombreMaterial: string;
  geometria:      Geometria;
  ladoA:          number;
  ladoB:          number;
  ladoC?:         number;
  ancho?:         number;
  precioPorM2:    number;
}

export interface DetalleCotizacionResponseDto {
  id:             number;
  nombreMaterial: string;
  geometria:      string;
  medidasJson:    string;
  precioPorM2:    number;
  areaTotal:      number;
  precioSubtotal: number;
}

// ─── Cotización ───────────────────────────────────────────────────────────────

export interface CotizacionResponseDto {
  id:              number;
  comentarios:     string | null;
  precioTotal:     number;
  estado:          EstadoCotizacion;
  fechaCreacion:   string;
  fechaAprobacion: string | null;
  cliente:         ClienteResponseDto;
  detalles:        DetalleCotizacionResponseDto[];
}
