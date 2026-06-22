import { useEffect, useState, useCallback } from 'react';
import { RefreshCw } from 'lucide-react';
import type { CotizacionKanbanDto } from '../../types/fabrica';
import { fabricaService } from './services/fabricaService';
import KanbanColumn from './components/KanbanColumn';

export default function FabricaDashboard() {
  const [porIniciar,   setPorIniciar]   = useState<CotizacionKanbanDto[]>([]);
  const [enProduccion, setEnProduccion] = useState<CotizacionKanbanDto[]>([]);
  const [finalizados,  setFinalizados]  = useState<CotizacionKanbanDto[]>([]);
  const [loading, setLoading]           = useState(true);
  const [ultimaActualizacion, setUltimaActualizacion] = useState<Date>(new Date());

  const cargar = useCallback(async () => {
    try {
      const [col1, col2, col3] = await Promise.all([
        fabricaService.getPorIniciar(),
        fabricaService.getEnProduccion(),
        fabricaService.getFinalizados(),
      ]);
      setPorIniciar(col1);
      setEnProduccion(col2);
      setFinalizados(col3);
      setUltimaActualizacion(new Date());
    } catch {
      /* silencioso en desarrollo */
    } finally {
      setLoading(false);
    }
  }, []);

  // Carga inicial
  useEffect(() => { cargar(); }, [cargar]);

  // Auto-refresco cada 30 segundos
  useEffect(() => {
    const interval = setInterval(cargar, 30_000);
    return () => clearInterval(interval);
  }, [cargar]);

  const handleIniciar = async (id: number) => {
    await fabricaService.iniciarOrden(id);
    await cargar();
  };

  const handleFinalizar = async (id: number) => {
    await fabricaService.finalizarOrden(id);
    await cargar();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Tablero de Producción</h1>
          <p className="text-slate-500 text-sm mt-1">
            Última actualización: {ultimaActualizacion.toLocaleTimeString('es-BO')}
          </p>
        </div>
        <button
          onClick={cargar}
          disabled={loading}
          className="flex items-center gap-2 text-sm text-slate-600 hover:text-slate-800 border border-slate-300 px-3 py-2 rounded-lg hover:bg-slate-50 transition disabled:opacity-50"
        >
          <RefreshCw size={15} className={loading ? 'animate-spin' : ''} />
          Actualizar
        </button>
      </div>

      {/* Tablero Kanban — 3 columnas */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        <KanbanColumn
          titulo="Por Iniciar"
          color="blue"
          ordenes={porIniciar}
          columna="porIniciar"
          onAccion={handleIniciar}
        />
        <KanbanColumn
          titulo="En Producción"
          color="orange"
          ordenes={enProduccion}
          columna="enProduccion"
          onAccion={handleFinalizar}
        />
        <KanbanColumn
          titulo="Finalizados"
          color="green"
          ordenes={finalizados}
          columna="finalizado"
          onAccion={async () => {}}
        />
      </div>
    </div>
  );
}
