import { useEffect, useState } from 'react';
import {
  getMicrosoftIntegration,
  saveMicrosoftIntegration,
  deleteMicrosoftIntegration,
  testMicrosoftConnection,
  type SaveMicrosoftIntegrationRequest,
} from '../services/microsoftIntegration';

export function IntegrationsPage() {
  const [configured, setConfigured] = useState(false);
  const [microsoftTenantId, setMicrosoftTenantId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);

  const [form, setForm] = useState<SaveMicrosoftIntegrationRequest>({
    microsoftTenantId: '',
    clientId: '',
    clientSecret: '',
  });

  const loadIntegration = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getMicrosoftIntegration();
      setConfigured(data.configured);
      setMicrosoftTenantId(data.microsoftTenantId ?? null);
      if (data.configured && data.microsoftTenantId) {
        setForm((prev) => ({ ...prev, microsoftTenantId: data.microsoftTenantId! }));
      } else {
        setForm({ microsoftTenantId: '', clientId: '', clientSecret: '' });
      }
    } catch (err: unknown) {
      const msg =
        (err instanceof Error ? err.message : null) ||
        'No se pudo cargar la configuración.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      void loadIntegration();
    }, 0);
    return () => clearTimeout(timer);
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    setError(null);
    setSuccess(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      await saveMicrosoftIntegration(form);
      await loadIntegration();
      setIsEditing(false);
      setSuccess('Credenciales guardadas correctamente.');
    } catch (err: unknown) {
      const msg =
        (err instanceof Error ? err.message : null) ||
        'No se pudieron guardar las credenciales.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      await deleteMicrosoftIntegration();
      await loadIntegration();
      setIsEditing(false);
      setSuccess('Integración eliminada.');
    } catch (err: unknown) {
      const msg =
        (err instanceof Error ? err.message : null) ||
        'No se pudo eliminar la integración.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleTest = async () => {
    setTesting(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await testMicrosoftConnection();
      setSuccess(result.message);
    } catch (err: unknown) {
      const msg =
        (err instanceof Error ? err.message : null) ||
        'La prueba de conexión falló.';
      setError(msg);
    } finally {
      setTesting(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-4">
        <div className="h-6 w-48 animate-pulse rounded-lg bg-slate-700" />
        <div className="h-4 w-64 animate-pulse rounded bg-slate-800" />
        <div className="h-32 max-w-xl animate-pulse rounded-xl bg-slate-800" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-white">Integraciones</h1>
        <p className="mt-1 text-sm text-slate-400">Conecta tu tenant Microsoft 365 para consultar incidencias reales.</p>
      </div>

      {error && (
        <div className="rounded-lg border border-red-900/40 bg-red-950/40 px-4 py-3 text-sm text-red-300">
          {error}
        </div>
      )}
      {success && (
        <div className="rounded-lg border border-emerald-900/40 bg-emerald-950/40 px-4 py-3 text-sm text-emerald-300">
          {success}
        </div>
      )}

      {configured && !isEditing ? (
        <div className="max-w-xl rounded-xl border border-slate-700 bg-slate-900/80 p-6 shadow-lg shadow-slate-950/20">
          <div className="flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-start sm:justify-between">
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <div className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500/10">
                  <svg className="h-4 w-4 text-blue-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round"><path d="M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z" /></svg>
                </div>
                <div className="text-sm font-semibold text-slate-100">Microsoft 365</div>
              </div>
              <div className="mt-1 text-xs text-slate-400">Tenant: {microsoftTenantId}</div>
              <div className="mt-2 inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-2.5 py-1 text-xs font-medium text-emerald-400 ring-1 ring-emerald-500/20">
                <span className="h-1.5 w-1.5 rounded-full bg-emerald-400" />
                Configurado
              </div>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                disabled={testing}
                onClick={handleTest}
                className="rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-200 transition-colors hover:bg-slate-700 hover:text-white disabled:opacity-50"
              >
                {testing ? 'Probando...' : 'Probar conexión'}
              </button>
              <button
                type="button"
                onClick={() => setIsEditing(true)}
                className="rounded-lg border border-blue-500/30 bg-blue-500/10 px-3 py-1.5 text-xs font-medium text-blue-400 transition-colors hover:bg-blue-500/20"
              >
                Editar
              </button>
              <button
                type="button"
                disabled={saving}
                onClick={handleDelete}
                className="rounded-lg border border-rose-500/30 bg-rose-500/10 px-3 py-1.5 text-xs font-medium text-rose-400 transition-colors hover:bg-rose-500/20 disabled:opacity-50"
              >
                Eliminar
              </button>
            </div>
          </div>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="max-w-xl space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-6 shadow-lg shadow-slate-950/20">
          <div>
            <label htmlFor="microsoftTenantId" className="mb-1.5 block text-sm font-medium text-slate-300">
              Microsoft Tenant ID
            </label>
            <input
              id="microsoftTenantId"
              name="microsoftTenantId"
              type="text"
              required
              value={form.microsoftTenantId}
              onChange={handleChange}
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2.5 text-sm text-slate-100 placeholder-slate-600 transition-colors focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500/50"
              placeholder="e.g. contoso.onmicrosoft.com o GUID"
            />
          </div>

          <div>
            <label htmlFor="clientId" className="mb-1.5 block text-sm font-medium text-slate-300">
              Client ID (App Registration)
            </label>
            <input
              id="clientId"
              name="clientId"
              type="text"
              required
              value={form.clientId}
              onChange={handleChange}
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2.5 text-sm text-slate-100 placeholder-slate-600 transition-colors focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500/50"
              placeholder="Application (client) ID"
            />
          </div>

          <div>
            <label htmlFor="clientSecret" className="mb-1.5 block text-sm font-medium text-slate-300">
              Client Secret
            </label>
            <input
              id="clientSecret"
              name="clientSecret"
              type="password"
              required
              value={form.clientSecret}
              onChange={handleChange}
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2.5 text-sm text-slate-100 placeholder-slate-600 transition-colors focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500/50"
              placeholder="Secret value"
            />
          </div>

          <div className="flex items-center gap-3 pt-2">
            <button
              type="submit"
              disabled={saving}
              className="rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-blue-500 disabled:opacity-50"
            >
              {saving ? 'Guardando...' : configured ? 'Actualizar credenciales' : 'Guardar credenciales'}
            </button>

            {configured && (
              <button
                type="button"
                onClick={() => setIsEditing(false)}
                className="rounded-lg border border-slate-700 bg-slate-800 px-4 py-2.5 text-sm font-medium text-slate-200 transition-colors hover:bg-slate-700 hover:text-white"
              >
                Cancelar
              </button>
            )}
          </div>
        </form>
      )}
    </div>
  );
}
