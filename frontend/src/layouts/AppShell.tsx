import { BarChart3, Briefcase, KanbanSquare, LogOut, Menu, Moon, Search, Settings, Sun, User, Users } from "lucide-react";
import { useState } from "react";
import { Link, NavLink, Outlet } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { routes } from "@/constants/routes";
import { useTheme } from "@/app/providers";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { cn, initials } from "@/lib/utils";

const navItems = [
  { to: routes.dashboard, label: "Dashboard", icon: BarChart3 },
  { to: routes.workspaces, label: "Workspaces", icon: Users },
  { to: routes.tasks, label: "My Tasks", icon: KanbanSquare },
  { to: routes.account, label: "Settings", icon: Settings },
];

export function AppShell() {
  const [open, setOpen] = useState(false);
  const { theme, setTheme } = useTheme();
  const { auth, logout } = useAuth();

  return (
    <div className="min-h-screen bg-background">
      <aside className={cn("fixed inset-y-0 left-0 z-30 w-64 border-r bg-card p-4 transition-transform lg:translate-x-0", open ? "translate-x-0" : "-translate-x-full")}>
        <Link to={routes.dashboard} className="mb-8 flex items-center gap-2 font-semibold">
          <span className="flex h-9 w-9 items-center justify-center rounded-md bg-primary text-primary-foreground"><Briefcase className="h-5 w-5" /></span>
          AgileFlow
        </Link>
        <nav className="grid gap-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === routes.dashboard}
              onClick={() => setOpen(false)}
              className={({ isActive }) => cn("flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-muted hover:text-foreground", isActive && "bg-muted text-foreground")}
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      {open ? <button className="fixed inset-0 z-20 bg-black/40 lg:hidden" aria-label="Close navigation" onClick={() => setOpen(false)} /> : null}
      <div className="lg:pl-64">
        <header className="sticky top-0 z-10 border-b bg-background/80 backdrop-blur">
          <div className="flex h-16 items-center gap-3 px-4 sm:px-6">
            <Button size="icon" variant="ghost" className="lg:hidden" onClick={() => setOpen(true)}><Menu className="h-5 w-5" /></Button>
            <div className="relative hidden max-w-md flex-1 sm:block">
              <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <input className="h-9 w-full rounded-md border bg-background pl-9 pr-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring" placeholder="Search workspaces, projects, tasks" />
            </div>
            <Button size="icon" variant="ghost" onClick={() => setTheme(theme === "dark" ? "light" : "dark")} aria-label="Toggle theme">
              {theme === "dark" ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            </Button>
            <Link to={routes.account} className="flex h-9 w-9 items-center justify-center rounded-full bg-secondary text-sm font-semibold" aria-label="Account">
              {auth?.email ? initials(auth.email) : <User className="h-4 w-4" />}
            </Link>
            <Button size="icon" variant="ghost" onClick={() => void logout()} aria-label="Logout"><LogOut className="h-5 w-5" /></Button>
          </div>
        </header>
        <main className="mx-auto max-w-7xl p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
