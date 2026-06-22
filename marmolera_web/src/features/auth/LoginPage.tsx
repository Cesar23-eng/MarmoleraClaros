import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore, UserRole } from '../../core/store/useAuthStore';
import { apiClient } from '../../core/api/apiClient';

// Cuentas mock para probar sin la API encendida
const MOCK_USERS: Record<string, { password: string; role: UserRole }> = {
  'admin@claros.com':       { password: '123456', role: 'Admin' },
  'ventas@claros.com':      { password: '123456', role: 'Ventas' },
  'fabrica@claros.com':     { password: '123456', role: 'Produccion' },
  'contabilidad@claros.com':{ password: '123456', role: 'Contabilidad' },
  'tablet@claros.com':      { password: '123456', role: 'Tablet' },
};

const ROLE_HOME: Record<UserRole, string> = {
  Admin: '/ventas',
  Ventas: '/ventas',
  Produccion: '/fabrica',
  Contabilidad: '/finanzas',
  Tablet: '/tablet',
};

export default function LoginPage() {
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [error, setError]       = useState('');
  const [loading, setLoading]   = useState(false);

  const setAuth  = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // Intentar contra la API real
      const { data } = await apiClient.post('/auth/login', { email, password });
      setAuth(data.token, data.role as UserRole, email);
      navigate(ROLE_HOME[data.role as UserRole] ?? '/ventas', { replace: true });
    } catch {
      // Fallback: mock local mientras la API no está levantada
      const mock = MOCK_USERS[email.toLowerCase()];
      if (mock && mock.password === password) {
        setAuth('mock-token', mock.role, email);
        navigate(ROLE_HOME[mock.role], { replace: true });
        return;
      }
      setError('Correo o contraseña incorrectos.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
      <div className="w-full max-w-sm">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-14 h-14 bg-blue-600 rounded-2xl mb-4 shadow-lg">
            <svg viewBox="0 0 32 32" fill="none" className="w-8 h-8 text-white" stroke="currentColor" strokeWidth="2">
              <polygon points="16,3 29,10 29,22 16,29 3,22 3,10" />
              <polygon points="16,9 23,13 23,19 16,23 9,19 9,13" fill="white" fillOpacity="0.2" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold text-slate-800">Marmolería Claros</h1>
          <p className="text-slate-500 text-sm mt-1">Ingresa a tu cuenta</p>
        </div>

        {/* Formulario */}
        <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-6">
          {error && (
            <div className="bg-red-50 border border-red-100 text-red-600 rounded-lg px-4 py-3 text-sm mb-5">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Correo electrónico</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="admin@claros.com"
                required
                className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Contraseña</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                required
                className="w-full px-3.5 py-2.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold py-2.5 rounded-lg transition disabled:opacity-60 disabled:cursor-not-allowed mt-2"
            >
              {loading ? 'Verificando...' : 'Iniciar sesión'}
            </button>
          </form>
        </div>

        {/* Hint desarrollo */}
        <p className="text-center text-xs text-slate-400 mt-4">
          Mock (sin API): admin@claros.com / 123456
        </p>
      </div>
    </div>
  );
}
