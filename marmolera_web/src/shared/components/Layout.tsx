import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useAuthStore, UserRole } from '../../core/store/useAuthStore';
import {
  LayoutDashboard,
  Factory,
  DollarSign,
  BarChart2,
  Tablet,
  Calendar,
  Bell,
  LogOut,
  ChevronRight,
} from 'lucide-react';

interface NavItem {
  label: string;
  path: string;
  icon: React.ComponentType<{ size?: number; className?: string }>;
  roles: UserRole[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Ventas',         path: '/ventas',         icon: LayoutDashboard, roles: ['Admin', 'Ventas'] },
  { label: 'Fábrica',        path: '/fabrica',        icon: Factory,         roles: ['Admin', 'Produccion'] },
  { label: 'Finanzas',       path: '/finanzas',       icon: DollarSign,      roles: ['Admin', 'Contabilidad'] },
  { label: 'Gerencia',       path: '/gerencia',       icon: BarChart2,       roles: ['Admin'] },
  { label: 'Tablet/Órdenes', path: '/tablet',         icon: Tablet,          roles: ['Admin', 'Tablet'] },
  { label: 'Calendario',     path: '/calendario',     icon: Calendar,        roles: ['Admin', 'Ventas', 'Produccion'] },
  { label: 'Notificaciones', path: '/notificaciones', icon: Bell,            roles: ['Admin', 'Ventas', 'Produccion', 'Contabilidad', 'Tablet'] },
];

export default function Layout() {
  const { role, email, logout } = useAuthStore();
  const navigate = useNavigate();

  const visibleItems = NAV_ITEMS.filter((item) =>
    role ? item.roles.includes(role) : false
  );

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  const initials = email ? email.substring(0, 2).toUpperCase() : '??';

  return (
    <div className="flex h-screen bg-gray-100 overflow-hidden">
      {/* ── Sidebar ── */}
      <aside className="w-60 shrink-0 bg-slate-900 text-white flex flex-col">
        {/* Logo */}
        <div className="px-6 py-5 border-b border-slate-800">
          <p className="text-lg font-bold tracking-wide">Marmolería Claros</p>
          <p className="text-xs text-slate-400 mt-0.5">Sistema ERP</p>
        </div>

        {/* Navegación */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
          {visibleItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.path}
                to={item.path}
                className={({ isActive }) =>
                  `group flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-400 hover:bg-slate-800 hover:text-white'
                  }`
                }
              >
                <Icon size={18} />
                <span className="flex-1">{item.label}</span>
                <ChevronRight size={14} className="opacity-0 group-hover:opacity-60 transition-opacity" />
              </NavLink>
            );
          })}
        </nav>

        {/* Perfil y Logout */}
        <div className="px-3 py-4 border-t border-slate-800 space-y-1">
          <div className="flex items-center gap-3 px-3 py-2">
            <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-xs font-bold shrink-0">
              {initials}
            </div>
            <div className="overflow-hidden">
              <p className="text-sm text-white font-medium truncate">{email}</p>
              <p className="text-xs text-slate-400">{role}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium text-red-400 hover:bg-slate-800 hover:text-red-300 transition-colors"
          >
            <LogOut size={18} />
            Cerrar Sesión
          </button>
        </div>
      </aside>

      {/* ── Contenido principal ── */}
      <main className="flex-1 overflow-y-auto">
        <div className="p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
