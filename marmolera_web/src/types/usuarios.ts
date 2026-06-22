export interface UsuarioDto {
  id: string;
  nombre: string;
  email: string;
  activo: boolean;
  fechaCreacion: string;
  roles: string[];
}

export interface CrearUsuarioDto {
  nombre: string;
  email: string;
  password: string;
  rol: string;
}

export interface EditarUsuarioDto {
  nombre: string;
  email: string;
  rol: string;
  activo: boolean;
}

export interface CambiarPasswordDto {
  nuevaPassword: string;
}

export const ROLES = ['Admin', 'Ventas', 'Produccion', 'Contabilidad', 'Tablet'] as const;
export type Rol = typeof ROLES[number];
