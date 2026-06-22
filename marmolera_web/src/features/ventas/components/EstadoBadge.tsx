import type { EstadoCotizacion } from '../../../types/ventas';

const config: Record<EstadoCotizacion, { label: string; className: string }> = {
  Cotizado:     { label: 'Cotizado',      className: 'bg-yellow-100 text-yellow-700' },
  Aprobado:     { label: 'Aprobado',      className: 'bg-blue-100 text-blue-700' },
  EnProduccion: { label: 'En Producción', className: 'bg-orange-100 text-orange-700' },
  Finalizado:   { label: 'Finalizado',    className: 'bg-green-100 text-green-700' },
};

export default function EstadoBadge({ estado }: { estado: string }) {
  const c = config[estado as EstadoCotizacion] ?? { label: estado, className: 'bg-gray-100 text-gray-600' };
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${c.className}`}>
      {c.label}
    </span>
  );
}
