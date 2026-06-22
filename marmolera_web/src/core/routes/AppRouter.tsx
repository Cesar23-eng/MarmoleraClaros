import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore, UserRole } from '../store/useAuthStore';
import Layout from '../../shared/components/Layout';
import LoginPage from '../../features/auth/LoginPage';
import VentasDashboard from '../../features/ventas/VentasDashboard';
import FabricaDashboard from '../../features/fabrica/FabricaDashboard';
import FinanzasDashboard from '../../features/finanzas/FinanzasDashboard';
import GerenciaDashboard from '../../features/gerencia/GerenciaDashboard';
import CalendarioPage from '../../features/calendario/CalendarioPage';
import NotificacionesPage from '../../features/notificaciones/NotificacionesPage';
import ReportesPage from '../../features/reportes/ReportesPage';
import TabletOrdenesPage from '../../features/ordenes/TabletOrdenesPage';
import UnauthorizedPage from '../../features/auth/UnauthorizedPage';

const roleHomePage: Record<UserRole, string> = {
  Admin: '/ventas',
  Ventas: '/ventas',
  Produccion: '/fabrica',
  Contabilidad: '/finanzas',
  Tablet: '/tablet',
};

function ProtectedRoute({
  children,
  allowedRoles,
}: {
  children: JSX.Element;
  allowedRoles?: UserRole[];
}) {
  const { isAuthenticated, role } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (allowedRoles && role && !allowedRoles.includes(role))
    return <Navigate to="/unauthorized" replace />;
  return children;
}

export const AppRouter = () => {
  const { isAuthenticated, role } = useAuthStore();

  return (
    <BrowserRouter>
      <Routes>
        {/* Ruta pública */}
        <Route
          path="/login"
          element={
            isAuthenticated && role
              ? <Navigate to={roleHomePage[role]} replace />
              : <LoginPage />
          }
        />

        <Route path="/unauthorized" element={<UnauthorizedPage />} />

        {/* Rutas protegidas bajo el Layout principal */}
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          {/* Redirección raíz por rol */}
          <Route
            index
            element={
              isAuthenticated && role
                ? <Navigate to={roleHomePage[role]} replace />
                : <Navigate to="/login" replace />
            }
          />

          <Route
            path="ventas"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Ventas']}>
                <VentasDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="fabrica"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Produccion']}>
                <FabricaDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="finanzas"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Contabilidad']}>
                <FinanzasDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="gerencia"
            element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <GerenciaDashboard />
              </ProtectedRoute>
            }
          />

          <Route
            path="tablet"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Tablet']}>
                <TabletOrdenesPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="calendario"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Ventas', 'Produccion']}>
                <CalendarioPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="notificaciones"
            element={
              <ProtectedRoute>
                <NotificacionesPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="reportes"
            element={
              <ProtectedRoute allowedRoles={['Admin', 'Contabilidad']}>
                <ReportesPage />
              </ProtectedRoute>
            }
          />
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
};
