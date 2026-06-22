import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';

// La URL base se lee del .env.local (VITE_API_URL)
// Si no existe, apunta al localhost de desarrollo
const BASE_URL = (import.meta as any).env?.VITE_API_URL ?? 'http://localhost:5183/api';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Inyectar JWT en cada petición automáticamente
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Si la API devuelve 401 (token expirado), cerrar sesión y redirigir al login
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
