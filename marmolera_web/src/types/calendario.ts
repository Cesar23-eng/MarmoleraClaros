export type TipoEvento = 'Entrega' | 'Instalacion' | 'Medicion' | 'Reunion' | 'Otro';

export interface EventoCalendarioDto {
  id: number;
  titulo: string;
  tipo: TipoEvento;
  fechaInicio: string;
  fechaFin: string;
  color?: string;
  notas?: string;
  usuarioId: string;
  pedidoId?: number;
  fueReprogramado: boolean;
  motivoReprogramacion?: string;
  fechaOriginal?: string;
  fechaCreacion: string;
}

export interface CrearEventoDto {
  titulo: string;
  tipo: TipoEvento;
  fechaInicio: string;
  fechaFin: string;
  color?: string;
  notas?: string;
  pedidoId?: number;
}

export interface ReprogramarEventoDto {
  nuevaFechaInicio: string;
  nuevaFechaFin: string;
  motivo: string;
}
