import { useNavigate } from 'react-router-dom';
import { ShieldOff } from 'lucide-react';

export default function UnauthorizedPage() {
  const navigate = useNavigate();
  return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-4 text-slate-500">
      <ShieldOff size={48} className="opacity-30" />
      <h2 className="text-xl font-semibold text-slate-700">Acceso no autorizado</h2>
      <p className="text-sm">No tienes permiso para ver esta sección.</p>
      <button
        onClick={() => navigate(-1)}
        className="text-sm text-blue-600 hover:underline"
      >
        Volver
      </button>
    </div>
  );
}
