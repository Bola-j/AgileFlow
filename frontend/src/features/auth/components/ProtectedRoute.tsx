import { Navigate, Outlet, useLocation } from "react-router-dom";
import { routes } from "@/constants/routes";
import { useAuth } from "@/features/auth/hooks/useAuth";

export function ProtectedRoute() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  if (!isAuthenticated) return <Navigate to={routes.login} replace state={{ from: location }} />;
  return <Outlet />;
}
