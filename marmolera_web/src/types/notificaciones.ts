export type TipoNotificacion =
  | 'CotizacionCreada'
  | 'CotizacionAprobada'
  | 'CotizacionRechazada'
  | 'PedidoCreado'
  | 'OrdenFabricaIniciada'
  | 'OrdenFabricaFinalizada'
  | 'General';

export interface NotificacionDto {
  id: number;
  titulo: string;
  mensaje: string;
  rolDestino: string;
  tipo: TipoNotificacion;
  referenciaId: number | null;
  leida: boolean;
  fechaCreacion: string;
}

export const ICONO_TIPO: Record<TipoNotificacion, string> = {
  CotizacionCreada:        '📋',
  CotizacionAprobada:      '✅',
  CotizacionRechazada:     '❌',
  PedidoCreado:            '🛒',
  OrdenFabricaIniciada:    '🔧',
  OrdenFabricaFinalizada:  '📦',
  General:                 '🔔',
};
