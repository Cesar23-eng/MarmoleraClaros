export interface MesonKanbanDto {
  id: number;
  nombreMaterial: string;
  geometria: string;
  areaM2: number;
}

export interface CotizacionKanbanDto {
  id: number;
  nombreCliente: string;
  telefono: string;
  fechaAprobacion?: string;
  comentarios?: string;
  cantidadMesones: number;
  mesones: MesonKanbanDto[];
}
