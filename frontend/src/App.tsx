import DashboardPage from "./pages/DashboardPage";
import ProjectsPage from "./pages/ProjectsPage";
import BacklogPage from "./pages/BacklogPage";

const navItems = ["Dashboard", "Projects", "Backlog"];

export default function App() {
  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-800 px-6 py-4">
        <div className="text-xl font-semibold">AgileFlow</div>
        <nav className="mt-2 flex gap-4 text-sm text-slate-400">
          {navItems.map((item) => (
            <span key={item} className="cursor-default">
              {item}
            </span>
          ))}
        </nav>
      </header>
      <main className="px-6 py-6 space-y-8">
        <DashboardPage />
        <ProjectsPage />
        <BacklogPage />
      </main>
    </div>
  );
}
