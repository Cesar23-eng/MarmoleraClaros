import { useEffect, useState, useCallback } from 'react';
import { ChevronLeft, ChevronRight, Plus, RefreshCw, AlertCircle } from 'lucide-react';
import type { EventoCalendarioDto, CrearEventoDto, TipoEvento } from '../../types/calendario';
import { calendarioService } from './services/calendarioService';
import EventoModal from './components/EventoModal';

// ─── Constantes ───────────────────────────────────────────────────────────────
const DIAS_SEMANA = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
const MESES = ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
               'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];

const COLOR_TIPO: Record<string, string> = {
  Entrega:    'bg-blue-500',
  Instalacion:'bg-purple-500',
  Medicion:   'bg-amber-500',
  Reunion:    'bg-emerald-500',
  Otro:       'bg-slate-400',
};

// ─── Helper: construir grilla del mes ─────────────────────────────────────────
function buildGrid(anio: number, mes: number): (Date | null)[] {
  const primerDia = new Date(anio, mes - 1, 1).getDay(); // 0=Dom
  const diasEnMes = new Date(anio, mes, 0).getDate();
  const grid: (Date | null)[] = [];

  for (let i = 0; i < primerDia; i++) grid.push(null);
  for (let d = 1; d <= diasEnMes; d++) grid.push(new Date(anio, mes - 1, d));
  while (grid.length % 7 !== 0) grid.push(null);

  return grid;
}

// ─── Componente principal ─────────────────────────────────────────────────────
export default function CalendarioPage() {
  const hoy      = new Date();
  const [anio,   setAnio]   = useState(hoy.getFullYear());
  const [mes,    setMes]    = useState(hoy.getMonth() + 1);
  const [eventos,setEventos]= useState<EventoCalendarioDto[]>([]);
  const [loading,setLoading]= useState(true);
  const [error,  setError]  = useState<string | null>(null);

  const [modalAbierto,   setModalAbierto]   = useState(false);
  const [eventoEditar,   setEventoEditar]   = useState<EventoCalendarioDto | null>(null);
  const [fechaClickeada, setFechaClickeada] = useState<string | undefined>();

  // ─── Carga eventos del mes ───────────────────────────────────────────────
  const cargar = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await calendarioService.getPorMes(anio, mes);
      setEventos(data);
    } catch {
      setError('No se pudieron cargar los eventos.');
    } finally {
      setLoading(false);
    }
  }, [anio, mes]);

  useEffect(() => { cargar(); }, [cargar]);

  // ─── Navegación mes ──────────────────────────────────────────────────────
  const irAnterior = () => {
    if (mes === 1) { setMes(12); setAnio(a => a - 1); }
    else setMes(m => m - 1);
  };
  const irSiguiente = () => {
    if (mes === 12) { setMes(1); setAnio(a => a + 1); }
    else setMes(m => m + 1);
  };

  // ─── Acciones CRUD ───────────────────────────────────────────────────────
  const handleGuardar = async (dto: CrearEventoDto) => {
    if (eventoEditar) {
      await calendarioService.actualizar(eventoEditar.id, dto);
    } else {
      await calendarioService.crear(dto);
    }
    setModalAbierto(false);
    setEventoEditar(null);
    await cargar();
  };

  const handleEliminar = async (id: number) => {
    if (!confirm('\u00bfEliminar este evento?')) return;
    await calendarioService.eliminar(id);
    await cargar();
  };

  const abrirNuevo = (fecha?: Date) => {
    setEventoEditar(null);
    setFechaClickeada(fecha ? fecha.toISOString().slice(0, 16) : undefined);
    setModalAbierto(true);
  };

  const abrirEditar = (e: EventoCalendarioDto) => {
    setEventoEditar(e);
    setFechaClickeada(undefined);
    setModalAbierto(true);
  };

  // ─── Grilla ───────────────────────────────────────────────────────────────
  const grid = buildGrid(anio, mes);

  const eventosDelDia = (fecha: Date) =>
    eventos.filter(ev => {
      const inicio = new Date(ev.fechaInicio);
      return inicio.getFullYear() === fecha.getFullYear() &&
             inicio.getMonth()    === fecha.getMonth()    &&
             inicio.getDate()     === fecha.getDate();
    });

  const esHoy = (fecha: Date) =>
    fecha.getFullYear() === hoy.getFullYear() &&
    fecha.getMonth()    === hoy.getMonth()    &&
    fecha.getDate()     === hoy.getDate();

  return (
    <div className="space-y-4">
      {/* ─── Header ─────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button onClick={irAnterior}
            className="p-2 rounded-lg border border-slate-200 hover:bg-slate-50 transition">
            <ChevronLeft size={18} />
          </button>
          <h1 className="text-xl font-bold text-slate-800 min-w-[200px] text-center">
            {MESES[mes - 1]} {anio}
          </h1>
          <button onClick={irSiguiente}
            className="p-2 rounded-lg border border-slate-200 hover:bg-slate-50 transition">
            <ChevronRight size={18} />
          </button>
          <button onClick={cargar} disabled={loading}
            className="p-2 rounded-lg border border-slate-200 hover:bg-slate-50 transition disabled:opacity-50 ml-1">
            <RefreshCw size={15} className={loading ? 'animate-spin' : ''} />
          </button>
        </div>

        <button onClick={() => abrirNuevo()}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-semibold transition">
          <Plus size={16} /> Nuevo evento
        </button>
      </div>

      {/* ─── Error ──────────────────────────────────────────────────── */}
      {error && (
        <div className="flex items-center gap-2 text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-3 text-sm">
          <AlertCircle size={16} /> {error}
        </div>
      )}

      {/* ─── Grilla calendario ──────────────────────────────────────── */}
      <div className="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm">
        {/* Cabecera días semana */}
        <div className="grid grid-cols-7 border-b border-slate-100">
          {DIAS_SEMANA.map(d => (
            <div key={d} className="py-2.5 text-center text-xs font-semibold text-slate-400 uppercase tracking-wide">
              {d}
            </div>
          ))}
        </div>

        {/* Celdas */}
        <div className="grid grid-cols-7">
          {grid.map((fecha, idx) => (
            <div
              key={idx}
              onClick={() => fecha && abrirNuevo(fecha)}
              className={`min-h-[100px] border-b border-r border-slate-100 p-1.5 ${
                fecha ? 'cursor-pointer hover:bg-slate-50 transition' : 'bg-slate-50/50'
              } ${idx % 7 === 6 ? 'border-r-0' : ''}`}
            >
              {fecha && (
                <>
                  {/* Número del día */}
                  <span className={`inline-flex items-center justify-center w-7 h-7 text-sm font-medium rounded-full mb-1 ${
                    esHoy(fecha)
                      ? 'bg-blue-600 text-white font-bold'
                      : 'text-slate-600 hover:bg-slate-200'
                  }`}>
                    {fecha.getDate()}
                  </span>

                  {/* Eventos del día */}
                  <div className="space-y-0.5">
                    {eventosDelDia(fecha).slice(0, 3).map(ev => (
                      <div
                        key={ev.id}
                        onClick={e => { e.stopPropagation(); abrirEditar(ev); }}
                        className={`flex items-center gap-1 px-1.5 py-0.5 rounded text-xs font-medium text-white truncate ${COLOR_TIPO[ev.tipo] ?? 'bg-slate-400'} cursor-pointer hover:opacity-80 transition`}
                        title={ev.titulo}
                      >
                        {ev.fueReprogramado && <span title="Reprogramado">&#8635;</span>}
                        <span className="truncate">{ev.titulo}</span>
                      </div>
                    ))}
                    {eventosDelDia(fecha).length > 3 && (
                      <p className="text-xs text-slate-400 pl-1">
                        +{eventosDelDia(fecha).length - 3} más
                      </p>
                    )}
                  </div>
                </>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* ─── Leyenda tipos ──────────────────────────────────────────── */}
      <div className="flex flex-wrap gap-3">
        {Object.entries(COLOR_TIPO).map(([tipo, bg]) => (
          <span key={tipo} className="flex items-center gap-1.5 text-xs text-slate-500">
            <span className={`w-2.5 h-2.5 rounded-full ${bg}`} />
            {tipo}
          </span>
        ))}
      </div>

      {/* ─── Modal ──────────────────────────────────────────────────── */}
      {modalAbierto && (
        <EventoModal
          evento={eventoEditar}
          fechaInicial={fechaClickeada}
          onGuardar={handleGuardar}
          onCerrar={() => { setModalAbierto(false); setEventoEditar(null); }}
        />
      )}
    </div>
  );
}
