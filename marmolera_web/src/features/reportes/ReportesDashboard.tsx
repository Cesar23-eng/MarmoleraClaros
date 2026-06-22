import { useEffect, useState } from 'react';
import { TrendingUp, Users, FileText, Package, RefreshCw } from 'lucide-react';
import type { ResumenGeneralDto, VentasMesDto, TopMaterialDto, EstadoDistribucionDto } from '../../types/reportes';
import { reportesService } from './services/reportesService';

// ─── Helpers ───────────────────────────────────────────────────────────────
const formatBs = (n: number) =>
  new Intl.NumberFormat('es-BO', { style: 'currency', currency: 'BOB', minimumFractionDigits: 0 }).format(n);

const COLOR_ESTADO: Record<string, string> = {
  Cotizado:      'bg-slate-400',
  Aprobado:      'bg-blue-500',
  EnProduccion:  'bg-orange-500',
  Finalizado:    'bg-green-500',
  Entregado:     'bg-purple-500',
};

// ─── KPI Card ───────────────────────────────────────────────────────────────
function KpiCard({
  titulo, valor, subtitulo, icono, color,
}: {
  titulo: string; valor: string; subtitulo?: string;
  icono: React.ReactNode; color: string;
}) {
  return (
    <div className="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wide">{titulo}</p>
          <p className="text-2xl font-bold text-slate-800 mt-1 tabular-nums">{valor}</p>
          {subtitulo && <p className="text-xs text-slate-400 mt-1">{subtitulo}</p>}
        </div>
        <div className={`p-2.5 rounded-xl ${color}`}>{icono}</div>
      </div>
    </div>
  );
}

// ─── Gráfico de barras SVG nativo ──────────────────────────────────────────
function BarChart({ datos }: { datos: VentasMesDto[] }) {
  const maxVal = Math.max(...datos.map(d => Number(d.total)), 1);
  const W = 600; const H = 160; const PAD = 32;
  const barW = (W - PAD * 2) / datos.length;

  return (
    <svg viewBox={`0 0 ${W} ${H + 30}`} className="w-full">
      {datos.map((d, i) => {
        const barH = Math.max(((Number(d.total) / maxVal) * H), 2);
        const x = PAD + i * barW + barW * 0.15;
        const y = H - barH;
        const w = barW * 0.7;
        return (
          <g key={i}>
            <rect x={x} y={y} width={w} height={barH}
              rx="3" fill={d.total > 0 ? '#3B82F6' : '#E2E8F0'}
              className="transition-all"
            />
            <text x={x + w / 2} y={H + 14} textAnchor="middle"
              fontSize="9" fill="#94A3B8">
              {d.mesNombre}
            </text>
            {d.total > 0 && (
              <text x={x + w / 2} y={y - 4} textAnchor="middle"
                fontSize="8" fill="#64748B" fontWeight="600">
                {(Number(d.total) / 1000).toFixed(0)}K
              </text>
            )}
          </g>
        );
      })}
    </svg>
  );
}

// ─── Dashboard principal ─────────────────────────────────────────────────────
export default function ReportesDashboard() {
  const [resumen,   setResumen]   = useState<ResumenGeneralDto | null>(null);
  const [ventas,    setVentas]    = useState<VentasMesDto[]>([]);
  const [materiales,setMateriales]= useState<TopMaterialDto[]>([]);
  const [estados,   setEstados]   = useState<EstadoDistribucionDto[]>([]);
  const [loading,   setLoading]   = useState(true);

  const cargar = async () => {
    try {
      setLoading(true);
      const [r, v, m, e] = await Promise.all([
        reportesService.getResumen(),
        reportesService.getVentasPorMes(12),
        reportesService.getTopMateriales(5),
        reportesService.getCotizacionesPorEstado(),
      ]);
      setResumen(r); setVentas(v); setMateriales(m); setEstados(e);
    } catch { /* silencioso */ }
    finally { setLoading(false); }
  };

  useEffect(() => { cargar(); }, []);

  if (loading) return (
    <div className="flex items-center justify-center h-64 text-slate-400 text-sm">Cargando reportes...</div>
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-800">Reportes</h1>
        <button onClick={cargar}
          className="flex items-center gap-2 text-sm text-slate-500 border border-slate-200 px-3 py-2 rounded-lg hover:bg-slate-50 transition">
          <RefreshCw size={14} /> Actualizar
        </button>
      </div>

      {/* KPIs */}
      {resumen && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <KpiCard
            titulo="Ingresos totales"
            valor={formatBs(resumen.ingresosTotales)}
            subtitulo={`Este mes: ${formatBs(resumen.ingresosEsteMes)}`}
            icono={<TrendingUp size={18} className="text-blue-600" />}
            color="bg-blue-50"
          />
          <KpiCard
            titulo="Cotizaciones"
            valor={String(resumen.totalCotizaciones)}
            subtitulo={`${resumen.cotizacionesAprobadas} aprobadas`}
            icono={<FileText size={18} className="text-green-600" />}
            color="bg-green-50"
          />
          <KpiCard
            titulo="En producción"
            valor={String(resumen.cotizacionesEnProduccion)}
            subtitulo={`${resumen.cotizacionesFinalizadas} finalizadas`}
            icono={<Package size={18} className="text-orange-600" />}
            color="bg-orange-50"
          />
          <KpiCard
            titulo="Clientes"
            valor={String(resumen.totalClientes)}
            subtitulo="Total registrados"
            icono={<Users size={18} className="text-purple-600" />}
            color="bg-purple-50"
          />
        </div>
      )}

      {/* Fila inferior */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Gráfico ventas */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-200 p-5 shadow-sm">
          <p className="text-sm font-semibold text-slate-700 mb-4">Ingresos últimos 12 meses (Bs.)</p>
          <BarChart datos={ventas} />
        </div>

        {/* Distribución estados */}
        <div className="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm">
          <p className="text-sm font-semibold text-slate-700 mb-4">Cotizaciones por estado</p>
          <div className="space-y-3">
            {estados.map(e => (
              <div key={e.estado}>
                <div className="flex justify-between text-xs text-slate-600 mb-1">
                  <span>{e.estado}</span>
                  <span className="font-semibold tabular-nums">{e.cantidad} ({e.porcentaje}%)</span>
                </div>
                <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all ${COLOR_ESTADO[e.estado] ?? 'bg-slate-400'}`}
                    style={{ width: `${e.porcentaje}%` }}
                  />
                </div>
              </div>
            ))}
            {estados.length === 0 && <p className="text-xs text-slate-400 text-center py-4">Sin datos</p>}
          </div>
        </div>
      </div>

      {/* Top materiales */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="px-5 py-4 border-b border-slate-100">
          <p className="text-sm font-semibold text-slate-700">Top 5 materiales más usados</p>
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 text-xs text-slate-500 uppercase tracking-wide">
              <th className="px-5 py-2.5 text-left">#</th>
              <th className="px-5 py-2.5 text-left">Material</th>
              <th className="px-5 py-2.5 text-right">Veces usado</th>
              <th className="px-5 py-2.5 text-right">Área total (m²)</th>
              <th className="px-5 py-2.5 text-right">Ingreso generado</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {materiales.map((m, i) => (
              <tr key={m.nombreMaterial} className="hover:bg-slate-50 transition">
                <td className="px-5 py-3 text-slate-400 font-medium">{i + 1}</td>
                <td className="px-5 py-3 font-medium text-slate-800">{m.nombreMaterial}</td>
                <td className="px-5 py-3 text-right tabular-nums text-slate-600">{m.vecesUsado}</td>
                <td className="px-5 py-3 text-right tabular-nums text-slate-600">{Number(m.areaTotalM2).toFixed(2)}</td>
                <td className="px-5 py-3 text-right tabular-nums font-semibold text-slate-800">{formatBs(m.ingresoGenerado)}</td>
              </tr>
            ))}
            {materiales.length === 0 && (
              <tr><td colSpan={5} className="px-5 py-8 text-center text-slate-400 text-xs">Sin datos todavía</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
