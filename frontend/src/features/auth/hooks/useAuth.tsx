import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { routes } from "@/constants/routes";
import { authApi } from "@/features/auth/api/authApi";
import { clearStoredAuth, getStoredAuth, setStoredAuth, toStoredAuth, type StoredAuth } from "@/services/authStorage";
import { getErrorMessage } from "@/services/apiClient";
import type { LoginRequestDto, RegisterRequestDto, RegisterResponseDto } from "@/types/api";

interface AuthContextValue {
  auth: StoredAuth | null;
  isAuthenticated: boolean;
  login: (payload: LoginRequestDto, remember: boolean) => Promise<void>;
  register: (payload: RegisterRequestDto) => Promise<RegisterResponseDto>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [auth, setAuth] = useState<StoredAuth | null>(() => getStoredAuth());
  const navigate = useNavigate();

  useEffect(() => {
    const onExpired = () => {
      setAuth(null);
      navigate(routes.login, { replace: true });
      toast.error("Session expired. Please sign in again.");
    };
    window.addEventListener("agileflow:auth-expired", onExpired);
    return () => window.removeEventListener("agileflow:auth-expired", onExpired);
  }, [navigate]);

  const persist = useCallback((next: StoredAuth) => {
    setStoredAuth(next);
    setAuth(next);
  }, []);

  const login = useCallback(
    async (payload: LoginRequestDto, remember: boolean) => {
      const response = await authApi.login(payload);
      persist(toStoredAuth(response, remember));
      toast.success("Signed in successfully.");
      navigate(routes.dashboard, { replace: true });
    },
    [navigate, persist],
  );

  const register = useCallback(
    async (payload: RegisterRequestDto) => {
      const response = await authApi.register(payload);
      toast.success("Account created. Check your email to verify it.");
      return response;
    },
    [],
  );

  const logout = useCallback(async () => {
    const current = getStoredAuth();
    try {
      if (current?.refreshToken) await authApi.logout({ refreshToken: current.refreshToken });
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      clearStoredAuth();
      setAuth(null);
      navigate(routes.login, { replace: true });
    }
  }, [navigate]);

  const value = useMemo(() => ({ auth, isAuthenticated: Boolean(auth), login, register, logout }), [auth, login, logout, register]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
