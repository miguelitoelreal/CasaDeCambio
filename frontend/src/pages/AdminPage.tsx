import { useEffect, useState } from "react";
import { Card } from "../components/Card";
import { Button } from "../components/Button";
import {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
  getEmailConfig,
  updateEmailConfig,
} from "../services/admin";
import type { UserListItem, TenantEmailConfig } from "../types/admin";

export function AdminPage() {
  const [activeTab, setActiveTab] = useState<"users" | "email">("users");

  const [users, setUsers] = useState<UserListItem[]>([]);
  const [usersLoading, setUsersLoading] = useState(false);
  const [usersError, setUsersError] = useState<string | null>(null);

  const [newName, setNewName] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [newIsAdmin, setNewIsAdmin] = useState(false);
  const [isCreatingUser, setIsCreatingUser] = useState(false);

  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editEmail, setEditEmail] = useState("");
  const [isEditingUser, setIsEditingUser] = useState(false);

  const [emailConfig, setEmailConfig] = useState<TenantEmailConfig>({
    smtpHost: "",
    smtpPort: 587,
    smtpUsername: "",
    smtpPassword: "",
    senderEmail: "",
    senderName: "",
    useSsl: true,
    emailEnabled: true,
  });
  const [emailLoading, setEmailLoading] = useState(false);
  const [emailSaving, setEmailSaving] = useState(false);
  const [emailError, setEmailError] = useState<string | null>(null);
  const [emailSuccess, setEmailSuccess] = useState<string | null>(null);

  useEffect(() => {
    document.title = "Cloud Alert Hub — Administración";
    loadUsers();
    loadEmailConfig();
  }, []);

  async function loadUsers() {
    setUsersLoading(true);
    setUsersError(null);
    try {
      const data = await getUsers();
      setUsers(data);
    } catch (err) {
      setUsersError(err instanceof Error ? err.message : "No se pudieron cargar los usuarios.");
    } finally {
      setUsersLoading(false);
    }
  }

  async function loadEmailConfig() {
    setEmailLoading(true);
    setEmailError(null);
    try {
      const data = await getEmailConfig();
      setEmailConfig(data);
    } catch (err) {
      setEmailError(err instanceof Error ? err.message : "No se pudo cargar la configuración.");
    } finally {
      setEmailLoading(false);
    }
  }

  async function handleCreateUser(e: React.FormEvent) {
    e.preventDefault();
    setIsCreatingUser(true);
    setUsersError(null);
    try {
      await createUser({
        name: newName.trim(),
        email: newEmail.trim(),
        password: newPassword,
        isAdmin: newIsAdmin,
      });
      setNewName("");
      setNewEmail("");
      setNewPassword("");
      setNewIsAdmin(false);
      await loadUsers();
    } catch (err) {
      setUsersError(err instanceof Error ? err.message : "No se pudo crear el usuario.");
    } finally {
      setIsCreatingUser(false);
    }
  }

  async function handleToggleAdmin(user: UserListItem) {
    try {
      await updateUser(user.id, { isAdmin: !user.isAdmin });
      await loadUsers();
    } catch (err) {
      setUsersError(err instanceof Error ? err.message : "No se pudo actualizar el usuario.");
    }
  }

  function handleStartEdit(user: UserListItem) {
    setEditingUserId(user.id);
    setEditName(user.name);
    setEditEmail(user.email);
    setUsersError(null);
  }

  function handleCancelEdit() {
    setEditingUserId(null);
    setEditName("");
    setEditEmail("");
  }

  async function handleSaveEdit(e: React.FormEvent, id: string) {
    e.preventDefault();
    setIsEditingUser(true);
    setUsersError(null);
    try {
      await updateUser(id, { name: editName.trim(), email: editEmail.trim() });
      setEditingUserId(null);
      await loadUsers();
    } catch (err) {
      setUsersError(err instanceof Error ? err.message : "No se pudo actualizar el usuario.");
    } finally {
      setIsEditingUser(false);
    }
  }

  async function handleDeleteUser(id: string) {
    if (!window.confirm("¿Eliminar este usuario?")) return;
    try {
      await deleteUser(id);
      await loadUsers();
    } catch (err) {
      setUsersError(err instanceof Error ? err.message : "No se pudo eliminar el usuario.");
    }
  }

  async function handleSaveEmail(e: React.FormEvent) {
    e.preventDefault();
    setEmailSaving(true);
    setEmailError(null);
    setEmailSuccess(null);
    try {
      await updateEmailConfig(emailConfig);
      setEmailSuccess("Configuración guardada correctamente.");
    } catch (err) {
      setEmailError(err instanceof Error ? err.message : "No se pudo guardar la configuración.");
    } finally {
      setEmailSaving(false);
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Administración</h1>

      <div className="flex gap-2 border-b border-gray-200">
        <button
          onClick={() => setActiveTab("users")}
          className={`px-4 py-2 text-sm font-medium ${
            activeTab === "users"
              ? "border-b-2 border-blue-600 text-blue-600"
              : "text-gray-500 hover:text-gray-700"
          }`}
        >
          Usuarios
        </button>
        <button
          onClick={() => setActiveTab("email")}
          className={`px-4 py-2 text-sm font-medium ${
            activeTab === "email"
              ? "border-b-2 border-blue-600 text-blue-600"
              : "text-gray-500 hover:text-gray-700"
          }`}
        >
          Configuración de email
        </button>
      </div>

      {activeTab === "users" && (
        <Card title="Gestión de usuarios">
          <div className="space-y-4">
            {usersError && (
              <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs font-medium text-red-700">
                {usersError}
              </div>
            )}

            <form onSubmit={handleCreateUser} className="flex flex-col gap-3 sm:flex-row sm:items-end">
              <div className="min-w-0 flex-1">
                <label className="block text-xs font-medium text-gray-700">Nombre</label>
                <input
                  type="text"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="Nombre completo"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div className="min-w-0 flex-1">
                <label className="block text-xs font-medium text-gray-700">Correo</label>
                <input
                  type="email"
                  value={newEmail}
                  onChange={(e) => setNewEmail(e.target.value)}
                  placeholder="correo@empresa.com"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div className="min-w-0 flex-1">
                <label className="block text-xs font-medium text-gray-700">Contraseña</label>
                <input
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="••••••••"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                  minLength={8}
                />
              </div>
              <div className="flex items-center gap-2 pb-2">
                <input
                  id="new-is-admin"
                  type="checkbox"
                  checked={newIsAdmin}
                  onChange={(e) => setNewIsAdmin(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <label htmlFor="new-is-admin" className="text-xs text-gray-700">
                  Admin
                </label>
              </div>
              <Button type="submit" isLoading={isCreatingUser}>
                Crear
              </Button>
            </form>

            <div className="overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead>
                  <tr className="border-b text-gray-500">
                    <th className="py-2 pr-4 font-medium">Nombre</th>
                    <th className="py-2 pr-4 font-medium">Correo</th>
                    <th className="py-2 pr-4 font-medium">Rol</th>
                    <th className="py-2 pr-4 font-medium">Creado</th>
                    <th className="py-2 font-medium">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {usersLoading ? (
                    <tr>
                      <td colSpan={5} className="py-3 text-xs text-gray-500">
                        Cargando usuarios…
                      </td>
                    </tr>
                  ) : users.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="py-3 text-xs text-gray-500">
                        No hay usuarios.
                      </td>
                    </tr>
                  ) : (
                    users.map((u) => (
                      <tr key={u.id} className="border-b">
                        {editingUserId === u.id ? (
                          <>
                            <td className="py-2 pr-4" colSpan={2}>
                              <form
                                onSubmit={(e) => handleSaveEdit(e, u.id)}
                                className="flex flex-wrap items-center gap-2"
                                id={`edit-form-${u.id}`}
                              >
                                <input
                                  type="text"
                                  value={editName}
                                  onChange={(e) => setEditName(e.target.value)}
                                  placeholder="Nombre"
                                  className="rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                  required
                                />
                                <input
                                  type="email"
                                  value={editEmail}
                                  onChange={(e) => setEditEmail(e.target.value)}
                                  placeholder="Correo"
                                  className="rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                                  required
                                />
                              </form>
                            </td>
                            <td className="py-2 pr-4">
                              <span
                                className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                                  u.isAdmin
                                    ? "bg-purple-100 text-purple-700"
                                    : "bg-gray-100 text-gray-700"
                                }`}
                              >
                                {u.isAdmin ? "Admin" : "Usuario"}
                              </span>
                            </td>
                            <td className="py-2 pr-4">
                              {new Date(u.createdAt).toLocaleDateString()}
                            </td>
                            <td className="py-2">
                              <div className="flex items-center gap-2">
                                <button
                                  type="submit"
                                  form={`edit-form-${u.id}`}
                                  disabled={isEditingUser}
                                  className="text-xs text-green-600 hover:underline disabled:opacity-50"
                                >
                                  {isEditingUser ? "Guardando..." : "Guardar"}
                                </button>
                                <button
                                  onClick={handleCancelEdit}
                                  className="text-xs text-gray-600 hover:underline"
                                >
                                  Cancelar
                                </button>
                              </div>
                            </td>
                          </>
                        ) : (
                          <>
                            <td className="py-2 pr-4">{u.name}</td>
                            <td className="py-2 pr-4">{u.email}</td>
                            <td className="py-2 pr-4">
                              <span
                                className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${
                                  u.isAdmin
                                    ? "bg-purple-100 text-purple-700"
                                    : "bg-gray-100 text-gray-700"
                                }`}
                              >
                                {u.isAdmin ? "Admin" : "Usuario"}
                              </span>
                            </td>
                            <td className="py-2 pr-4">
                              {new Date(u.createdAt).toLocaleDateString()}
                            </td>
                            <td className="py-2">
                              <div className="flex items-center gap-2">
                                <button
                                  onClick={() => handleStartEdit(u)}
                                  className="text-xs text-blue-600 hover:underline"
                                >
                                  Editar
                                </button>
                                <button
                                  onClick={() => handleToggleAdmin(u)}
                                  className="text-xs text-blue-600 hover:underline"
                                >
                                  {u.isAdmin ? "Quitar admin" : "Hacer admin"}
                                </button>
                                <button
                                  onClick={() => handleDeleteUser(u.id)}
                                  className="text-xs text-red-600 hover:underline"
                                >
                                  Eliminar
                                </button>
                              </div>
                            </td>
                          </>
                        )}
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </Card>
      )}

      {activeTab === "email" && (
        <Card title="Configuración de correo (SMTP)">
          {emailLoading ? (
            <div className="text-xs text-gray-500">Cargando configuración…</div>
          ) : (
          <form onSubmit={handleSaveEmail} className="space-y-4">
            {emailError && (
              <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs font-medium text-red-700">
                {emailError}
              </div>
            )}
            {emailSuccess && (
              <div className="rounded-md border border-green-200 bg-green-50 px-3 py-2 text-xs font-medium text-green-700">
                {emailSuccess}
              </div>
            )}

            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <label className="block text-xs font-medium text-gray-700">Servidor SMTP</label>
                <input
                  type="text"
                  value={emailConfig.smtpHost}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, smtpHost: e.target.value }))}
                  placeholder="smtp.gmail.com"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700">Puerto</label>
                <input
                  type="number"
                  value={emailConfig.smtpPort}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, smtpPort: Number(e.target.value) }))}
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700">Usuario SMTP</label>
                <input
                  type="text"
                  value={emailConfig.smtpUsername}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, smtpUsername: e.target.value }))}
                  placeholder="correo@gmail.com"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700">Contraseña SMTP</label>
                <input
                  type="password"
                  value={emailConfig.smtpPassword}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, smtpPassword: e.target.value }))}
                  placeholder="••••••••"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700">Correo remitente</label>
                <input
                  type="email"
                  value={emailConfig.senderEmail}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, senderEmail: e.target.value }))}
                  placeholder="noreply@empresa.com"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700">Nombre remitente</label>
                <input
                  type="text"
                  value={emailConfig.senderName}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, senderName: e.target.value }))}
                  placeholder="Cloud Alert Hub"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  required
                />
              </div>
            </div>

            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <input
                  id="use-ssl"
                  type="checkbox"
                  checked={emailConfig.useSsl}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, useSsl: e.target.checked }))}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <label htmlFor="use-ssl" className="text-xs text-gray-700">
                  Usar SSL/TLS
                </label>
              </div>
              <div className="flex items-center gap-2">
                <input
                  id="email-enabled"
                  type="checkbox"
                  checked={emailConfig.emailEnabled}
                  onChange={(e) => setEmailConfig((c) => ({ ...c, emailEnabled: e.target.checked }))}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <label htmlFor="email-enabled" className="text-xs text-gray-700">
                  Habilitar envío de emails
                </label>
              </div>
            </div>

            <div className="flex justify-end">
              <Button type="submit" isLoading={emailSaving}>
                Guardar configuración
              </Button>
            </div>
          </form>
          )}
        </Card>
      )}
    </div>
  );
}
