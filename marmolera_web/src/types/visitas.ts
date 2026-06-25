export interface VisitaResponseDto {
  eventoId:        number;
  cotizacionId:    number;
  clienteNombre:   string;
  clienteTelefono: string;
  clienteDireccion: string;
  fechaVisita:     string;
  estadoVisita:    'Pendiente' | 'Confirmada' | 'Realizada' | 'Cancelada';
  notas:           string | null;
  fueReprogramada: boolean;
  fechaOriginal:   string | null;
}
