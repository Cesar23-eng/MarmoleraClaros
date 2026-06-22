import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type UserRole = 'Admin' | 'Ventas' | 'Produccion' | 'Contabilidad' | 'Tablet';

interface AuthState {
  token: string | null;
  role: UserRole | null;
  email: string | null;
  isAuthenticated: boolean;
  setAuth: (token: string, role: UserRole, email: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      role: null,
      email: null,
      isAuthenticated: false,
      setAuth: (token, role, email) =>
        set({ token, role, email, isAuthenticated: true }),
      logout: () =>
        set({ token: null, role: null, email: null, isAuthenticated: false }),
    }),
    { name: 'marmolera-auth-storage' }
  )
);
