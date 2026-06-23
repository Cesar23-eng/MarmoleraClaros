import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';

const BASE_URL = (import.meta as any).env?.VITE_API_URL ?? 'http://localhost:5183/api';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Inyectar JWT en cada petición
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Si la API devuelve 401, intentar renovar con refreshToken antes de cerrar sesión
let renovando = false;

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // Si es 401, no es retry, y tenemos refreshToken → intentar renovar
    if (error.response?.status === 401 && !original._retry) {
      const { refreshToken, setAuth, logout, role, email } = useAuthStore.getState();

      // Si no hay refreshToken o ya estamos renovando, cerrar sesión
      if (!refreshToken || renovando) {
        logout();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        renovando = true;
        original._retry = true;

        const { data } = await axios.post(`${BASE_URL}/auth/refresh`, { refreshToken });

        // Actualizar store con los nuevos tokens
        setAuth(data.accessToken, data.roles?.[0] ?? role, data.email ?? email, data.refreshToken);

        // Reintentar la petición original con el nuevo token
        original.headers.Authorization = `Bearer ${data.accessToken}`;
        return apiClient(original);
      } catch {
        logout();
        window.location.href = '/login';
        return Promise.reject(error);
      } finally {
        renovando = false;
      }
    }

    return Promise.reject(error);
  }
);
