import { useState } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { SidebarNav } from "../components/SidebarNav";
import { Topbar } from "../components/Topbar";

export function DashboardLayout() {
  const location = useLocation();
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);

  const title = location.pathname.startsWith("/monitors/")
    ? "Monitor"
    : location.pathname.startsWith("/centro-estado-cloud")
      ? "Centro de Estado Cloud"
      : location.pathname.startsWith("/configuracion")
        ? "Configuración"
        : location.pathname.startsWith("/admin")
          ? "Administración"
          : "Centro de Monitoreo";

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="flex min-h-screen">
        <SidebarNav
          isOpen={mobileSidebarOpen}
          onClose={() => setMobileSidebarOpen(false)}
        />
        <div className="flex min-w-0 flex-1 flex-col">
          <Topbar
            title={title}
            onMenuClick={() => setMobileSidebarOpen((s) => !s)}
          />
          <main className="flex-1 p-4 md:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
