import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "sonner";
import { AuthProvider } from "@/features/auth/hooks/useAuth";

export function AppProviders({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState(() => localStorage.getItem("agileflow.theme") ?? "system");
  const queryClient = useMemo(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: { staleTime: 30_000, refetchOnWindowFocus: false, retry: 1 },
        },
      }),
    [],
  );

  useMemo(() => {
    const dark = theme === "dark" || (theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches);
    document.documentElement.classList.toggle("dark", dark);
    localStorage.setItem("agileflow.theme", theme);
  }, [theme]);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <ThemeContext.Provider value={{ theme, setTheme }}>
            {children}
            <Toaster richColors position="top-right" />
          </ThemeContext.Provider>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

import { createContext, useContext } from "react";

const ThemeContext = createContext<{ theme: string; setTheme: (theme: string) => void } | null>(null);

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) throw new Error("useTheme must be used within AppProviders");
  return context;
}
