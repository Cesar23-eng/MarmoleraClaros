import type { PlantillaGeometria } from '../../../types/ventas';

interface Props {
  geometria: PlantillaGeometria;
  ladoA: number;
  ladoB: number;
  ladoC?: number;
  ancho?: number;
}

/** Calcula el área igual que el backend para mostrarlo en tiempo real */
export function calcularArea({ geometria, ladoA, ladoB, ladoC, ancho }: Props): number {
  if (!ladoA || !ladoB) return 0;
  switch (geometria) {
    case 'Rectangulo':
      return ladoA * ladoB;
    case 'Forma_L':
      if (!ancho) return 0;
      return ladoA * ancho + (ladoB - ancho) * ancho;
    case 'Forma_U':
      if (!ancho || !ladoC) return 0;
      return ladoA * ancho + (ladoB - 2 * ancho) * ancho + ladoC * ancho;
    default:
      return 0;
  }
}

export default function AreaPreview(props: Props) {
  const area = calcularArea(props);
  return (
    <div className="text-right">
      <span className="text-xs text-slate-500">Área calculada: </span>
      <span className="text-sm font-semibold text-slate-700">{area.toFixed(4)} m²</span>
    </div>
  );
}
