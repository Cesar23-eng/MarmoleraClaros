# Marmolería Claros — Frontend Web (React)

Frontend ERP construido con **React 18 + TypeScript + Vite + Tailwind CSS**.

## Stack
- React 18 + TypeScript
- Vite (bundler)
- Tailwind CSS v3
- React Router v6 (rutas protegidas por rol)
- Zustand (store de autenticación con persistencia)
- Axios (cliente HTTP con interceptor JWT)

## Estructura
```
src/
├── core/
│   ├── api/        # Axios client (JWT automático)
│   ├── store/      # Zustand auth store
│   └── routes/     # AppRouter + ProtectedRoute
├── features/
│   ├── auth/       # LoginPage + UnauthorizedPage
│   ├── ventas/     # Dashboard de ventas
│   ├── fabrica/    # Control de producción
│   ├── finanzas/   # Contabilidad
│   ├── gerencia/   # Vista ejecutiva
│   ├── ordenes/    # Tablet de órdenes
│   ├── calendario/ # Calendario de eventos
│   ├── notificaciones/
│   └── reportes/
└── shared/
    └── components/ # Layout global (sidebar + outlet)
```

## Levantar en desarrollo
```bash
cd marmolera_web
npm install
npm run dev
# Abre http://localhost:5173
```

## Variables de entorno
Copia `.env.example` como `.env.local` y ajusta la URL de tu API:
```
VITE_API_URL=http://localhost:5183/api
```

## Mock Login (sin API)
| Email | Password | Rol |
|---|---|---|
| admin@claros.com | 123456 | Admin |
| ventas@claros.com | 123456 | Ventas |
| fabrica@claros.com | 123456 | Produccion |
| contabilidad@claros.com | 123456 | Contabilidad |
| tablet@claros.com | 123456 | Tablet |
