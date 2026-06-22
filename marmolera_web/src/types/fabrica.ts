export interface CotizacionKanbanDto {
  ordenFabricaId: number;
  cotizacionId: number;
  nombreCliente: string;
  telefono: string;
  estado: 'PorIniciar' | 'EnProduccion' | 'Finalizado';
  operarioNombre: string | null;
  notas: string | null;
  precioTotal: number;
  fechaCreacion: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  totalPiezas: number;
}

export interface AsignarOperarioDto {
  operarioId: string;
  operarioNombre: string;
}

export interface AgregarNotaDto {
  nota: string;
}
