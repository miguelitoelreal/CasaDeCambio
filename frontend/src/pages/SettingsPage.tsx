import { useEffect, useState } from "react";
import { Card } from "../components/Card";
import { Button } from "../components/Button";
import { FormField } from "../components/FormField";
import { useAuth } from "../auth/useAuth";
import { authService } from "../services/auth";
import {
  getMyAlertPreferences,
  updateMyAlertPreferences,
  getCloudProviderOptions,
  sendTestAlert,
} from "../services/alerts";
import { SummaryFrequency } from "../types/alerts";
import type { UserAlertPreference, CloudProviderOption } from "../types/alerts";

export function SettingsPage() {
  const { user } = useAuth();

  const [name, setName] = useState(user?.name ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [profileError, setProfileError] = useState<string | null>(null);
  const [profileSuccess, setProfileSuccess] = useState<string | null>(null);
  const [isSavingProfile, setIsSavingProfile] = useState(false);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [showPasswordForm, setShowPasswordForm] = useState(false);

  const [emailError, setEmailError] = useState<string | null>(null);
  const [emailSuccess, setEmailSuccess] = useState<string | null>(null);
  const [showEmailForm, setShowEmailForm] = useState(false);

  const [alertError, setAlertError] = useState<string | null>(null);
  const [alertSuccess, setAlertSuccess] = useState<string | null>(null);

  const [preferences, setPreferences] = useState<UserAlertPreference>({
    emailEnabled: true,
    monitorDownAlerts: true,
    cloudIncidentCriticalAlerts: true,
    cloudIncidentMajorAlerts: true,
    summaryEnabled: false,
    summaryFrequency: SummaryFrequency.Weekly,
    summaryDay: 1,
    summaryIncludeMonitors: true,
    summaryIncludeCloud: true,
    selectedCloudProviderIds: [],
    additionalEmails: [],
  });
  const [prefLoading, setPrefLoading] = useState(false);
  const [cloudProviders, setCloudProviders] = useState<CloudProviderOption[]>([]);

  useEffect(() => {
    loadPreferences();
  }, []);

  async function loadPreferences() {
    setPrefLoading(true);
    try {
      const [prefs, providers] = await Promise.all([
        getMyAlertPreferences(),
        getCloudProviderOptions(),
      ]);
      setPreferences(prefs);
      setCloudProviders(providers);
    } catch {
      // preferences may not exist yet, use defaults
    } finally {
      setPrefLoading(false);
    }
  }

  async function handleSavePreferences() {
    setAlertError(null);
    setAlertSuccess(null);
    try {
      await updateMyAlertPreferences(preferences);
      setAlertSuccess("Preferencias guardadas correctamente.");
    } catch (err) {
      setAlertError(
        err instanceof Error
          ? err.message
          : "No se pudieron guardar las preferencias.",
      );
    }
  }

  const [testAlertLoading, setTestAlertLoading] = useState(false);
  const [testAlertType, setTestAlertType] = useState<"monitor" | "critical" | "major">("monitor");

  async function handleTestAlert() {
    setAlertError(null);
    setAlertSuccess(null);
    setTestAlertLoading(true);
    try {
      const result = await sendTestAlert(testAlertType);
      setAlertSuccess(`${result.message} Destinatarios: ${result.recipients.join(", ")}`);
    } catch (err) {
      setAlertError(
        err instanceof Error ? err.message : "No se pudo enviar la alerta de prueba.",
      );
    } finally {
      setTestAlertLoading(false);
    }
  }

  async function handleSaveProfile(e: React.FormEvent) {
    e.preventDefault();
    setProfileError(null);
    setProfileSuccess(null);
    setIsSavingProfile(true);

    try {
      await authService.updateProfile({ name, email });
      setProfileSuccess("Perfil actualizado correctamente.");
    } catch (err) {
      setProfileError(
        err instanceof Error ? err.message : "No se pudo actualizar el perfil.",
      );
    } finally {
      setIsSavingProfile(false);
    }
  }

  async function handleSaveEmail() {
    setEmailError(null);
    setEmailSuccess(null);
    try {
      await authService.updateProfile({ name, email });
      setEmailSuccess("Correo actualizado correctamente.");
      setShowEmailForm(false);
    } catch (err) {
      setEmailError(
        err instanceof Error ? err.message : "No se pudo actualizar el correo.",
      );
    }
  }

  async function handleChangePassword(e: React.FormEvent) {
    e.preventDefault();
    setPasswordError(null);
    setPasswordSuccess(null);

    if (newPassword !== confirmPassword) {
      setPasswordError("Las contraseñas nuevas no coinciden.");
      return;
    }
    if (newPassword.length < 8) {
      setPasswordError("La contraseña debe tener al menos 8 caracteres.");
      return;
    }

    setIsChangingPassword(true);
    try {
      await authService.changePassword({
        currentPassword,
        newPassword,
      });
      setPasswordSuccess("Contraseña cambiada correctamente.");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err) {
      setPasswordError(
        err instanceof Error
          ? err.message
          : "No se pudo cambiar la contraseña.",
      );
    } finally {
      setIsChangingPassword(false);
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight text-white">Configuración</h1>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Left column — Notifications (takes 2/3 on large screens) */}
        <div className="lg:col-span-2 space-y-6">
          <Card title="Centro de notificaciones">
            <div className="space-y-5">
              {prefLoading ? (
                <div className="text-xs text-slate-400">Cargando preferencias…</div>
              ) : (
                <>
                  {/* Step 1 — Master toggle */}
                  <div className="flex items-center justify-between rounded-xl border border-blue-900/30 bg-blue-900/20 px-4 py-3">
                    <div className="flex items-center gap-3">
                      <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-900/30 text-blue-400">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                          <path strokeLinecap="round" strokeLinejoin="round" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                        </svg>
                      </div>
                      <div>
                        <p className="text-sm font-medium text-slate-100">Activar alertas por email</p>
                        <p className="text-xs text-slate-400">Paso 1 — Activa esto primero para poder configurar todo lo demás</p>
                      </div>
                    </div>
                    <label className="relative inline-flex cursor-pointer items-center">
                      <input
                        type="checkbox"
                        checked={preferences.emailEnabled}
                        onChange={(e) => setPreferences((p) => ({ ...p, emailEnabled: e.target.checked }))}
                        className="peer sr-only"
                      />
                      <div className="h-6 w-11 rounded-full bg-slate-700 peer-checked:bg-blue-600 peer-focus:ring-4 peer-focus:ring-blue-900/40 after:absolute after:left-[2px] after:top-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-slate-700 after:bg-slate-900/60 after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-blue-400" />
                    </label>
                  </div>

                  {!preferences.emailEnabled && (
                    <div className="rounded-md border border-amber-900/30 bg-amber-900/20 px-4 py-3 text-xs text-amber-300">
                      Las alertas por email están desactivadas. Actívalas arriba para configurar tus notificaciones.
                    </div>
                  )}

                  {/* Steps 2 & 3 side-by-side */}
                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    {/* Step 2 — Additional recipients */}
                    <div className="rounded-xl border border-slate-700 bg-slate-900/60">
                      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">2</span>
                        <div>
                          <h3 className="text-sm font-semibold text-slate-200">Destinatarios adicionales</h3>
                          <p className="text-xs text-slate-400">Quién más recibirá tus alertas</p>
                        </div>
                      </div>
                      <div className="px-4 py-3">
                        <input
                          type="text"
                          value={preferences.additionalEmails.join(", ")}
                          onChange={(e) =>
                            setPreferences((p) => ({
                              ...p,
                              additionalEmails: e.target.value.split(",").map((s) => s.trim()).filter(Boolean),
                            }))
                          }
                          disabled={!preferences.emailEnabled}
                          placeholder="correo1@empresa.com, correo2@empresa.com"
                          className="block w-full rounded-md border border-slate-700 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:bg-slate-900/40 disabled:text-slate-500"
                        />
                      </div>
                    </div>

                    {/* Step 3 — Alert types */}
                    <div className="rounded-xl border border-slate-700 bg-slate-900/60">
                      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">3</span>
                        <div>
                          <h3 className="text-sm font-semibold text-slate-200">Tipos de alerta</h3>
                          <p className="text-xs text-slate-400">Eventos que te notifican</p>
                        </div>
                      </div>
                      <div className="space-y-1 px-4 py-3">
                        {[
                          { key: "monitorDownAlerts", label: "Monitor caído", desc: "Cuando un monitor deje de responder" },
                          { key: "cloudIncidentCriticalAlerts", label: "Incidencias críticas cloud", desc: "Problemas graves reportados por proveedores cloud" },
                          { key: "cloudIncidentMajorAlerts", label: "Incidencias mayores cloud", desc: "Interrupciones significativas de servicios cloud" },
                        ].map((item) => (
                          <label key={item.key} className="flex cursor-pointer items-start gap-3 rounded-md p-2 hover:bg-slate-900/40">
                            <input
                              type="checkbox"
                              checked={preferences[item.key as keyof UserAlertPreference] as boolean}
                              onChange={(e) =>
                                setPreferences((p) => ({ ...p, [item.key]: e.target.checked }))
                              }
                              disabled={!preferences.emailEnabled}
                              className="mt-0.5 h-4 w-4 rounded border-slate-700 text-blue-600 focus:ring-blue-500 disabled:opacity-40"
                            />
                            <div className={preferences.emailEnabled ? "" : "opacity-40"}>
                              <p className="text-sm font-medium text-slate-300">{item.label}</p>
                              <p className="text-xs text-slate-400">{item.desc}</p>
                            </div>
                          </label>
                        ))}
                      </div>
                    </div>
                  </div>

                  {/* Step 4 — Cloud providers */}
                  {cloudProviders.length > 0 && (
                    <div className="rounded-xl border border-slate-700 bg-slate-900/60">
                      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">4</span>
                        <div>
                          <h3 className="text-sm font-semibold text-slate-200">Proveedores cloud</h3>
                          <p className="text-xs text-slate-400">Elige de qué proveedores quieres recibir alertas de incidencias</p>
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2 px-4 py-3">
                        {cloudProviders.map((provider) => (
                          <label
                            key={provider.id}
                            className={`inline-flex cursor-pointer items-center gap-1.5 rounded-full border px-3 py-1.5 text-xs font-medium transition-colors ${
                              preferences.selectedCloudProviderIds.includes(provider.id)
                                ? "border-blue-500/30 bg-blue-500/10 text-blue-400"
                                : "border-slate-700 bg-slate-900/40 text-slate-400 hover:bg-slate-800/60"
                            } ${!preferences.emailEnabled ? "opacity-40" : ""}`}
                          >
                            <input
                              type="checkbox"
                              checked={preferences.selectedCloudProviderIds.includes(provider.id)}
                              onChange={(e) => {
                                const ids = new Set(preferences.selectedCloudProviderIds);
                                if (e.target.checked) ids.add(provider.id);
                                else ids.delete(provider.id);
                                setPreferences((p) => ({ ...p, selectedCloudProviderIds: Array.from(ids) }));
                              }}
                              disabled={!preferences.emailEnabled}
                              className="sr-only"
                            />
                            <span className={`h-2 w-2 rounded-full ${preferences.selectedCloudProviderIds.includes(provider.id) ? "bg-blue-500" : "bg-gray-300"}`} />
                            {provider.name}
                          </label>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Steps 5 & 6 side-by-side */}
                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    {/* Step 5 — Summary */}
                    <div className="rounded-xl border border-slate-700 bg-slate-900/60">
                      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">5</span>
                        <div className="flex-1">
                          <h3 className="text-sm font-semibold text-slate-200">Resumen de alertas</h3>
                          <p className="text-xs text-slate-400">Resumen periódico por email</p>
                        </div>
                        <label className="relative inline-flex cursor-pointer items-center">
                          <input
                            type="checkbox"
                            checked={preferences.summaryEnabled}
                            onChange={(e) => setPreferences((p) => ({ ...p, summaryEnabled: e.target.checked }))}
                            disabled={!preferences.emailEnabled}
                            className="peer sr-only"
                          />
                          <div className="h-6 w-11 rounded-full bg-slate-700 peer-checked:bg-blue-600 peer-focus:ring-4 peer-focus:ring-blue-900/40 after:absolute after:left-[2px] after:top-[2px] after:h-5 after:w-5 after:rounded-full after:border after:border-slate-700 after:bg-slate-900/60 after:transition-all after:content-[''] peer-checked:after:translate-x-full peer-checked:after:border-blue-400 disabled:opacity-40" />
                        </label>
                      </div>
                      {preferences.summaryEnabled && preferences.emailEnabled && (
                        <div className="space-y-3 px-4 py-3">
                          <div className="flex flex-wrap items-center gap-3">
                            <div>
                              <label className="block text-xs font-medium text-slate-400">Frecuencia</label>
                              <select
                                value={preferences.summaryFrequency}
                                onChange={(e) =>
                                  setPreferences((p) => ({ ...p, summaryFrequency: Number(e.target.value) as SummaryFrequency }))
                                }
                                className="mt-1 rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-xs text-slate-100"
                              >
                                <option value={SummaryFrequency.Daily}>Diario</option>
                                <option value={SummaryFrequency.Weekly}>Semanal</option>
                                <option value={SummaryFrequency.Monthly}>Mensual</option>
                              </select>
                            </div>
                            {preferences.summaryFrequency !== SummaryFrequency.Daily && (
                              <div>
                                <label className="block text-xs font-medium text-slate-400">Día de envío</label>
                                <select
                                  value={preferences.summaryDay}
                                  onChange={(e) => setPreferences((p) => ({ ...p, summaryDay: Number(e.target.value) }))}
                                  className="mt-1 rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-xs text-slate-100"
                                >
                                  <option value={0}>Domingo</option>
                                  <option value={1}>Lunes</option>
                                  <option value={2}>Martes</option>
                                  <option value={3}>Miércoles</option>
                                  <option value={4}>Jueves</option>
                                  <option value={5}>Viernes</option>
                                  <option value={6}>Sábado</option>
                                </select>
                              </div>
                            )}
                          </div>
                          <div className="flex items-center gap-4">
                            <label className="inline-flex items-center gap-1.5 text-xs">
                              <input
                                type="checkbox"
                                checked={preferences.summaryIncludeMonitors}
                                onChange={(e) => setPreferences((p) => ({ ...p, summaryIncludeMonitors: e.target.checked }))}
                                className="h-3.5 w-3.5 rounded border-slate-700 text-blue-600"
                              />
                              Incluir monitores
                            </label>
                            <label className="inline-flex items-center gap-1.5 text-xs">
                              <input
                                type="checkbox"
                                checked={preferences.summaryIncludeCloud}
                                onChange={(e) => setPreferences((p) => ({ ...p, summaryIncludeCloud: e.target.checked }))}
                                className="h-3.5 w-3.5 rounded border-slate-700 text-blue-600"
                              />
                              Incluir proveedores cloud
                            </label>
                          </div>
                        </div>
                      )}
                    </div>

                    {/* Step 6 — Test alert */}
                    <div className="rounded-xl border border-slate-700 bg-slate-900/60">
                      <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
                        <span className="flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-[10px] font-bold text-white">6</span>
                        <div>
                          <h3 className="text-sm font-semibold text-slate-200">Probar alertas</h3>
                          <p className="text-xs text-slate-400">Verifica que tu configuración funciona</p>
                        </div>
                      </div>
                      <div className="flex flex-col gap-3 px-4 py-3 sm:flex-row sm:items-center">
                        <select
                          value={testAlertType}
                          onChange={(e) => setTestAlertType(e.target.value as "monitor" | "critical" | "major")}
                          disabled={!preferences.emailEnabled || testAlertLoading}
                          className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 disabled:bg-slate-900/40"
                        >
                          <option value="monitor">Monitor caído</option>
                          <option value="critical">Incidencia crítica</option>
                          <option value="major">Incidencia mayor</option>
                        </select>
                        <Button
                          type="button"
                          onClick={handleTestAlert}
                          isLoading={testAlertLoading}
                          disabled={!preferences.emailEnabled}
                        >
                          Enviar alerta de prueba
                        </Button>
                      </div>
                    </div>
                  </div>

                  {alertSuccess ? (
                    <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-xs font-medium text-emerald-700">
                      {alertSuccess}
                    </div>
                  ) : null}
                  {alertError ? (
                    <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs font-medium text-red-700">
                      {alertError}
                    </div>
                  ) : null}

                  <div className="flex justify-end pt-2">
                    <Button type="button" onClick={handleSavePreferences}>
                      Guardar preferencias
                    </Button>
                  </div>
                </>
              )}
            </div>
          </Card>
        </div>

        {/* Right column — Profile & Security (takes 1/3 on large screens) */}
        <div className="space-y-6">
          <Card title="Perfil y cuenta">
            <div className="space-y-4">
              <form onSubmit={handleSaveProfile} className="space-y-4">
                <FormField label="Nombre completo" htmlFor="settings-name">
                  <input
                    id="settings-name"
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                    minLength={2}
                    maxLength={120}
                    className="block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  />
                </FormField>

                <FormField label="Empresa / Tenant" htmlFor="settings-tenant">
                  <input
                    id="settings-tenant"
                    type="text"
                    value={user?.tenantName ?? ""}
                    disabled
                    className="block w-full rounded-md border border-slate-700 bg-slate-900/40 px-3 py-2 text-sm text-slate-400 shadow-sm"
                  />
                </FormField>

                {profileError ? (
                  <div className="rounded-md border border-red-900/40 bg-red-950/40 px-3 py-2 text-xs font-medium text-red-300">
                    {profileError}
                  </div>
                ) : null}
                {profileSuccess ? (
                  <div className="rounded-md border border-emerald-900/40 bg-emerald-950/40 px-3 py-2 text-xs font-medium text-emerald-300">
                    {profileSuccess}
                  </div>
                ) : null}

                <div className="flex justify-end">
                  <Button type="submit" isLoading={isSavingProfile}>
                    Guardar cambios
                  </Button>
                </div>
              </form>

              {!showEmailForm ? (
                <div className="flex items-center justify-between rounded-md border border-slate-700 bg-slate-900/40 px-3 py-2">
                  <div>
                    <p className="text-xs font-medium text-slate-300">Correo electrónico</p>
                    <p className="text-xs text-slate-400">{email}</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => { setShowEmailForm(true); setEmailError(null); setEmailSuccess(null); }}
                    className="text-xs font-medium text-blue-400 hover:text-blue-300"
                  >
                    Editar
                  </button>
                </div>
              ) : (
                <form onSubmit={(e) => { e.preventDefault(); handleSaveEmail(); }} className="space-y-2">
                  <FormField label="Correo electrónico" htmlFor="settings-email">
                    <input
                      id="settings-email"
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      required
                      maxLength={256}
                      className="block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </FormField>
                  {emailError ? (
                    <div className="rounded-md border border-red-900/40 bg-red-950/40 px-3 py-2 text-xs font-medium text-red-300">
                      {emailError}
                    </div>
                  ) : null}
                  {emailSuccess ? (
                    <div className="rounded-md border border-emerald-900/40 bg-emerald-950/40 px-3 py-2 text-xs font-medium text-emerald-300">
                      {emailSuccess}
                    </div>
                  ) : null}
                  <div className="flex gap-2">
                    <Button type="button" variant="secondary" onClick={() => setShowEmailForm(false)}>
                      Cancelar
                    </Button>
                    <Button type="submit">
                      Guardar correo
                    </Button>
                  </div>
                </form>
              )}
            </div>
          </Card>

          <Card title="Seguridad">
            <div className="space-y-3">
              {!showPasswordForm ? (
                <div className="flex items-center justify-between rounded-md border border-slate-700 bg-slate-900/40 px-3 py-2">
                  <div>
                    <p className="text-xs font-medium text-slate-300">Contraseña</p>
                    <p className="text-xs text-slate-400">••••••••</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => { setShowPasswordForm(true); setPasswordError(null); setPasswordSuccess(null); }}
                    className="text-xs font-medium text-blue-400 hover:text-blue-300"
                  >
                    Cambiar
                  </button>
                </div>
              ) : (
                <form onSubmit={handleChangePassword} className="space-y-3">
                  <FormField label="Contraseña actual" htmlFor="settings-current-password">
                    <input
                      id="settings-current-password"
                      type="password"
                      value={currentPassword}
                      onChange={(e) => setCurrentPassword(e.target.value)}
                      required
                      className="block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </FormField>

                  <FormField label="Nueva contraseña" htmlFor="settings-new-password">
                    <input
                      id="settings-new-password"
                      type="password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      required
                      minLength={8}
                      className="block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </FormField>

                  <FormField label="Confirmar nueva contraseña" htmlFor="settings-confirm-password">
                    <input
                      id="settings-confirm-password"
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                      minLength={8}
                      className="block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                  </FormField>

                  {passwordError ? (
                    <div className="rounded-md border border-red-900/40 bg-red-950/40 px-3 py-2 text-xs font-medium text-red-300">
                      {passwordError}
                    </div>
                  ) : null}
                  {passwordSuccess ? (
                    <div className="rounded-md border border-emerald-900/40 bg-emerald-950/40 px-3 py-2 text-xs font-medium text-emerald-300">
                      {passwordSuccess}
                    </div>
                  ) : null}

                  <div className="flex gap-2">
                    <Button type="button" variant="secondary" onClick={() => setShowPasswordForm(false)}>
                      Cancelar
                    </Button>
                    <Button type="submit" isLoading={isChangingPassword}>
                      Cambiar contraseña
                    </Button>
                  </div>
                </form>
              )}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
