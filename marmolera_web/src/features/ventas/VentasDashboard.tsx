import { FileText, Users, TrendingUp } from 'lucide-react';

const kpis = [
  { label: 'Cotizaciones Activas', value: '—', icon: FileText, color: 'blue' },
  { label: 'Clientes Registrados', value: '—', icon: Users,    color: 'emerald' },
  { label: 'Ventas del Mes',       value: '—', icon: TrendingUp, color: 'violet' },
];

const colorMap: Record<string, string> = {
  blue:    'bg-blue-50 text-blue-600',
  emerald: 'bg-emerald-50 text-emerald-600',
  violet:  'bg-violet-50 text-violet-600',
};

export default function VentasDashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">Dashboard de Ventas</h1>
        <p className="text-slate-500 text-sm mt-1">Gestión de cotizaciones y clientes</p>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {kpis.map((kpi) => {
          const Icon = kpi.icon;
          return (
            <div key={kpi.label} className="bg-white rounded-xl border border-slate-200 p-5 flex items-center gap-4">
              <div className={`w-11 h-11 rounded-lg flex items-center justify-center shrink-0 ${colorMap[kpi.color]}`}>
                <Icon size={22} />
              </div>
              <div>
                <p className="text-xs font-medium text-slate-500">{kpi.label}</p>
                <p className="text-2xl font-bold text-slate-800 mt-0.5">{kpi.value}</p>
              </div>
            </div>
          );
        })}
      </div>

      {/* Placeholder tabla */}
      <div className="bg-white rounded-xl border border-slate-200">
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <h2 className="font-semibold text-slate-700">Cotizaciones Recientes</h2>
          <button className="text-sm bg-blue-600 text-white px-4 py-1.5 rounded-lg hover:bg-blue-700 transition">
            + Nueva cotización
          </button>
        </div>
        <div className="flex items-center justify-center h-52 text-slate-400 text-sm">
          La tabla de cotizaciones se conectará a la API en el siguiente paso.
        </div>
      </div>
    </div>
  );
}
