export type TipoNotificacion =
  | 'NuevaCotizacion'
  | 'CotizacionAprobada'
  | 'OrdenIniciada'
  | 'OrdenFinalizada'
  | 'EventoProximo'
  | 'General';

export interface NotificacionDto {
  id: number;
  tipo: TipoNotificacion;
  mensaje: string;
  destinoRol: string;
  leida: boolean;
  fechaCreacion: string;
  referenciaId?: number;
}
