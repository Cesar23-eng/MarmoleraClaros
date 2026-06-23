import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type UserRole = 'Admin' | 'Ventas' | 'Produccion' | 'Contabilidad' | 'Tablet';

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  role: UserRole | null;
  email: string | null;
  isAuthenticated: boolean;
  setAuth: (token: string, role: UserRole, email: string, refreshToken?: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      role: null,
      email: null,
      isAuthenticated: false,
      setAuth: (token, role, email, refreshToken) =>
        set({ token, role, email, refreshToken: refreshToken ?? null, isAuthenticated: true }),
      logout: () =>
        set({ token: null, refreshToken: null, role: null, email: null, isAuthenticated: false }),
    }),
    { name: 'marmolera-auth-storage' }
  )
);
