import { useEffect, useState } from 'react';
import { clientesService } from '../services/clientesService';
import type { ClienteResponseDto } from '../../../types/ventas';

export function useClientes() {
  const [clientes, setClientes] = useState<ClienteResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const cargar = async () => {
    try {
      setLoading(true);
      const data = await clientesService.getAll();
      setClientes(data);
    } catch {
      setError('No se pudieron cargar los clientes.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { cargar(); }, []);

  return { clientes, loading, error, recargar: cargar };
}
