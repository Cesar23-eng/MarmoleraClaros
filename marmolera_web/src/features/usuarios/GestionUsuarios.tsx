import { useEffect, useState } from 'react';
import { UserPlus, Pencil, KeyRound, UserX, ShieldCheck, RefreshCw } from 'lucide-react';
import type { UsuarioDto, CrearUsuarioDto, EditarUsuarioDto } from '../../types/usuarios';
import { ROLES } from '../../types/usuarios';
import { usuariosService } from './services/usuariosService';

const ROL_COLOR: Record<string, string> = {
  Admin:        'bg-red-100 text-red-700',
  Ventas:       'bg-blue-100 text-blue-700',
  Produccion:   'bg-orange-100 text-orange-700',
  Contabilidad: 'bg-green-100 text-green-700',
  Tablet:       'bg-purple-100 text-purple-700',
};

// ─── Modal genérico ────────────────────────────────────────────────────────────
function Modal({ titulo, onClose, children }: { titulo: string; onClose: () => void; children: React.ReactNode }) {
  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 className="font-semibold text-slate-800">{titulo}</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600 text-xl leading-none">&times;</button>
        </div>
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  );
}

// ─── Formulario Crear ─────────────────────────────────────────────────────────
function FormCrear({ onGuardar, onCancelar }: { onGuardar: (dto: CrearUsuarioDto) => Promise<void>; onCancelar: () => void }) {
  const [form, setForm] = useState<CrearUsuarioDto>({ nombre: '', email: '', password: '', rol: 'Ventas' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true); setError('');
    try { await onGuardar(form); }
    catch (err: any) { setError(err?.response?.data?.mensaje ?? 'Error al crear usuario'); }
    finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && <p className="text-sm text-red-600 bg-red-50 px-3 py-2 rounded-lg">{error}</p>}
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Nombre completo</label>
        <input className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.nombre} onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))} required />
      </div>
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Email</label>
        <input type="email" className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} required />
      </div>
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Contraseña</label>
        <input type="password" className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} required minLength={6} />
      </div>
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Rol</label>
        <select className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.rol} onChange={e => setForm(f => ({ ...f, rol: e.target.value }))}>
          {ROLES.map(r => <option key={r}>{r}</option>)}
        </select>
      </div>
      <div className="flex gap-2 pt-2">
        <button type="submit" disabled={loading}
          className="flex-1 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium py-2 rounded-lg transition disabled:opacity-50">
          {loading ? 'Creando...' : 'Crear usuario'}
        </button>
        <button type="button" onClick={onCancelar}
          className="flex-1 border border-slate-300 text-slate-700 text-sm py-2 rounded-lg hover:bg-slate-50 transition">
          Cancelar
        </button>
      </div>
    </form>
  );
}

// ─── Formulario Editar ─────────────────────────────────────────────────────────
function FormEditar({ usuario, onGuardar, onCancelar }: { usuario: UsuarioDto; onGuardar: (dto: EditarUsuarioDto) => Promise<void>; onCancelar: () => void }) {
  const [form, setForm] = useState<EditarUsuarioDto>({
    nombre: usuario.nombre,
    email:  usuario.email,
    rol:    usuario.roles[0] ?? 'Ventas',
    activo: usuario.activo,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true); setError('');
    try { await onGuardar(form); }
    catch { setError('Error al guardar cambios'); }
    finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && <p className="text-sm text-red-600 bg-red-50 px-3 py-2 rounded-lg">{error}</p>}
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Nombre completo</label>
        <input className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.nombre} onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))} required />
      </div>
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Email</label>
        <input type="email" className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} required />
      </div>
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Rol</label>
        <select className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={form.rol} onChange={e => setForm(f => ({ ...f, rol: e.target.value }))}>
          {ROLES.map(r => <option key={r}>{r}</option>)}
        </select>
      </div>
      <div className="flex items-center gap-2">
        <input type="checkbox" id="activo" checked={form.activo}
          onChange={e => setForm(f => ({ ...f, activo: e.target.checked }))} />
        <label htmlFor="activo" className="text-sm text-slate-700">Usuario activo</label>
      </div>
      <div className="flex gap-2 pt-2">
        <button type="submit" disabled={loading}
          className="flex-1 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium py-2 rounded-lg transition disabled:opacity-50">
          {loading ? 'Guardando...' : 'Guardar cambios'}
        </button>
        <button type="button" onClick={onCancelar}
          className="flex-1 border border-slate-300 text-slate-700 text-sm py-2 rounded-lg hover:bg-slate-50 transition">
          Cancelar
        </button>
      </div>
    </form>
  );
}

// ─── Formulario Cambiar Password ──────────────────────────────────────────────────
function FormPassword({ usuarioNombre, onGuardar, onCancelar }: { usuarioNombre: string; onGuardar: (pwd: string) => Promise<void>; onCancelar: () => void }) {
  const [pwd, setPwd]     = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true); setError('');
    try { await onGuardar(pwd); }
    catch { setError('Error al cambiar contraseña'); }
    finally { setLoading(false); }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <p className="text-sm text-slate-500">Nueva contraseña para <strong className="text-slate-800">{usuarioNombre}</strong></p>
      {error && <p className="text-sm text-red-600 bg-red-50 px-3 py-2 rounded-lg">{error}</p>}
      <div>
        <label className="block text-xs font-medium text-slate-600 mb-1">Nueva contraseña</label>
        <input type="password" className="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={pwd} onChange={e => setPwd(e.target.value)} required minLength={6} />
      </div>
      <div className="flex gap-2 pt-2">
        <button type="submit" disabled={loading}
          className="flex-1 bg-orange-600 hover:bg-orange-700 text-white text-sm font-medium py-2 rounded-lg transition disabled:opacity-50">
          {loading ? 'Cambiando...' : 'Cambiar contraseña'}
        </button>
        <button type="button" onClick={onCancelar}
          className="flex-1 border border-slate-300 text-slate-700 text-sm py-2 rounded-lg hover:bg-slate-50 transition">
          Cancelar
        </button>
      </div>
    </form>
  );
}

// ─── Página principal ────────────────────────────────────────────────────────────
export default function GestionUsuarios() {
  const [usuarios, setUsuarios] = useState<UsuarioDto[]>([]);
  const [loading,  setLoading]  = useState(true);
  const [modal, setModal] = useState<'crear' | 'editar' | 'password' | null>(null);
  const [seleccionado, setSeleccionado] = useState<UsuarioDto | null>(null);

  const cargar = async () => {
    try { setLoading(true); setUsuarios(await usuariosService.listar()); }
    catch { /* silencioso */ }
    finally { setLoading(false); }
  };

  useEffect(() => { cargar(); }, []);

  const handleCrear = async (dto: CrearUsuarioDto) => {
    await usuariosService.crear(dto);
    await cargar();
    setModal(null);
  };

  const handleEditar = async (dto: any) => {
    await usuariosService.editar(seleccionado!.id, dto);
    await cargar();
    setModal(null);
  };

  const handlePassword = async (pwd: string) => {
    await usuariosService.cambiarPassword(seleccionado!.id, { nuevaPassword: pwd });
    setModal(null);
  };

  const handleDesactivar = async (u: UsuarioDto) => {
    if (!confirm(`¿Desactivar a ${u.nombre}?`)) return;
    await usuariosService.desactivar(u.id);
    await cargar();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Usuarios</h1>
          <p className="text-slate-500 text-sm mt-1">{usuarios.length} usuario{usuarios.length !== 1 ? 's' : ''} registrados</p>
        </div>
        <div className="flex gap-2">
          <button onClick={cargar} className="flex items-center gap-2 text-sm text-slate-500 border border-slate-200 px-3 py-2 rounded-lg hover:bg-slate-50 transition">
            <RefreshCw size={14} />
          </button>
          <button onClick={() => setModal('crear')}
            className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition">
            <UserPlus size={15} /> Nuevo usuario
          </button>
        </div>
      </div>

      {/* Tabla */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        {loading ? (
          <div className="py-16 text-center text-slate-400 text-sm">Cargando usuarios...</div>
        ) : usuarios.length === 0 ? (
          <div className="py-16 text-center text-slate-400 text-sm">No hay usuarios registrados</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 text-xs text-slate-500 uppercase tracking-wide">
                <th className="px-5 py-3 text-left">Nombre</th>
                <th className="px-5 py-3 text-left">Email</th>
                <th className="px-5 py-3 text-left">Rol</th>
                <th className="px-5 py-3 text-left">Estado</th>
                <th className="px-5 py-3 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {usuarios.map(u => (
                <tr key={u.id} className={`hover:bg-slate-50 transition ${!u.activo ? 'opacity-50' : ''}`}>
                  <td className="px-5 py-3">
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 rounded-full bg-slate-200 flex items-center justify-center text-xs font-bold text-slate-600">
                        {u.nombre.charAt(0).toUpperCase()}
                      </div>
                      <span className="font-medium text-slate-800">{u.nombre}</span>
                    </div>
                  </td>
                  <td className="px-5 py-3 text-slate-500">{u.email}</td>
                  <td className="px-5 py-3">
                    {u.roles.map(r => (
                      <span key={r} className={`text-xs font-medium px-2 py-0.5 rounded-full ${ROL_COLOR[r] ?? 'bg-slate-100 text-slate-600'}`}>
                        {r}
                      </span>
                    ))}
                  </td>
                  <td className="px-5 py-3">
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                      u.activo ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                    }`}>
                      {u.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="px-5 py-3">
                    <div className="flex items-center justify-end gap-1">
                      <button title="Editar" onClick={() => { setSeleccionado(u); setModal('editar'); }}
                        className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition">
                        <Pencil size={14} />
                      </button>
                      <button title="Cambiar contraseña" onClick={() => { setSeleccionado(u); setModal('password'); }}
                        className="p-1.5 text-slate-400 hover:text-orange-600 hover:bg-orange-50 rounded-lg transition">
                        <KeyRound size={14} />
                      </button>
                      {u.activo && (
                        <button title="Desactivar" onClick={() => handleDesactivar(u)}
                          className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition">
                          <UserX size={14} />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modales */}
      {modal === 'crear' && (
        <Modal titulo="Nuevo usuario" onClose={() => setModal(null)}>
          <FormCrear onGuardar={handleCrear} onCancelar={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'editar' && seleccionado && (
        <Modal titulo="Editar usuario" onClose={() => setModal(null)}>
          <FormEditar usuario={seleccionado} onGuardar={handleEditar} onCancelar={() => setModal(null)} />
        </Modal>
      )}
      {modal === 'password' && seleccionado && (
        <Modal titulo="Cambiar contraseña" onClose={() => setModal(null)}>
          <FormPassword usuarioNombre={seleccionado.nombre} onGuardar={handlePassword} onCancelar={() => setModal(null)} />
        </Modal>
      )}
    </div>
  );
}
