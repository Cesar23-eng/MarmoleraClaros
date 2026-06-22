import type { DetalleCotizacionCreateDto, PlantillaGeometria } from '../../../types/ventas';
import AreaPreview from './AreaPreview';
import { Trash2 } from 'lucide-react';

const GEOMETRIAS: { value: PlantillaGeometria; label: string }[] = [
  { value: 'Rectangulo', label: 'Rectángulo' },
  { value: 'Forma_L',    label: 'Forma L' },
  { value: 'Forma_U',    label: 'Forma U' },
];

interface Props {
  index: number;
  detalle: DetalleCotizacionCreateDto;
  onChange: (index: number, campo: keyof DetalleCotizacionCreateDto, valor: string | number | undefined) => void;
  onRemove: (index: number) => void;
  canRemove: boolean;
}

export default function MesonForm({ index, detalle, onChange, onRemove, canRemove }: Props) {
  const inputCls = 'w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition';
  const labelCls = 'block text-xs font-medium text-slate-600 mb-1';

  return (
    <div className="border border-slate-200 rounded-xl p-4 bg-slate-50 space-y-3">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-semibold text-slate-700">Mesón #{index + 1}</h4>
        {canRemove && (
          <button
            type="button"
            onClick={() => onRemove(index)}
            className="text-red-400 hover:text-red-600 transition"
          >
            <Trash2 size={16} />
          </button>
        )}
      </div>

      {/* Material y Geometría */}
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className={labelCls}>Material / Descripción</label>
          <input
            type="text"
            value={detalle.nombreMaterial}
            onChange={(e) => onChange(index, 'nombreMaterial', e.target.value)}
            placeholder="Ej: Mármol Blanco Carrara"
            className={inputCls}
            required
          />
        </div>
        <div>
          <label className={labelCls}>Geometría</label>
          <select
            value={detalle.geometria}
            onChange={(e) => onChange(index, 'geometria', e.target.value as PlantillaGeometria)}
            className={inputCls}
          >
            {GEOMETRIAS.map((g) => (
              <option key={g.value} value={g.value}>{g.label}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Medidas según geometría */}
      <div className="grid grid-cols-4 gap-3">
        <div>
          <label className={labelCls}>Lado A (m)</label>
          <input
            type="number" step="0.01" min="0"
            value={detalle.ladoA || ''}
            onChange={(e) => onChange(index, 'ladoA', parseFloat(e.target.value) || 0)}
            className={inputCls} required
          />
        </div>
        <div>
          <label className={labelCls}>Lado B (m)</label>
          <input
            type="number" step="0.01" min="0"
            value={detalle.ladoB || ''}
            onChange={(e) => onChange(index, 'ladoB', parseFloat(e.target.value) || 0)}
            className={inputCls} required
          />
        </div>

        {/* LadoC solo para Forma U */}
        {detalle.geometria === 'Forma_U' && (
          <div>
            <label className={labelCls}>Lado C (m)</label>
            <input
              type="number" step="0.01" min="0"
              value={detalle.ladoC || ''}
              onChange={(e) => onChange(index, 'ladoC', parseFloat(e.target.value) || undefined)}
              className={inputCls} required
            />
          </div>
        )}

        {/* Ancho para Forma L y U */}
        {(detalle.geometria === 'Forma_L' || detalle.geometria === 'Forma_U') && (
          <div>
            <label className={labelCls}>Ancho (m)</label>
            <input
              type="number" step="0.01" min="0"
              value={detalle.ancho || ''}
              onChange={(e) => onChange(index, 'ancho', parseFloat(e.target.value) || undefined)}
              className={inputCls} required
            />
          </div>
        )}

        <div>
          <label className={labelCls}>Precio / m² (Bs)</label>
          <input
            type="number" step="0.01" min="0"
            value={detalle.precioPorM2 || ''}
            onChange={(e) => onChange(index, 'precioPorM2', parseFloat(e.target.value) || 0)}
            className={inputCls} required
          />
        </div>
      </div>

      {/* Preview área */}
      <AreaPreview
        geometria={detalle.geometria}
        ladoA={detalle.ladoA}
        ladoB={detalle.ladoB}
        ladoC={detalle.ladoC}
        ancho={detalle.ancho}
      />
    </div>
  );
}
