import { Navigate, Route, Routes } from "react-router-dom";
import { AppProviders } from "@/app/providers";
import { routes } from "@/constants/routes";
import { ProtectedRoute } from "@/features/auth/components/ProtectedRoute";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { OAuthCallbackPage } from "@/features/auth/pages/OAuthCallbackPage";
import { RegisterPage } from "@/features/auth/pages/RegisterPage";
import { VerifyEmailPage } from "@/features/auth/pages/VerifyEmailPage";
import { AppShell } from "@/layouts/AppShell";
import { DashboardPage } from "@/pages/DashboardPage";
import { AccountPage } from "@/features/account/pages/AccountPage";
import { BoardPage } from "@/features/board/pages/BoardPage";
import { ProjectDetailsPage } from "@/features/projects/pages/ProjectDetailsPage";
import { ProjectsPage } from "@/features/projects/pages/ProjectsPage";
import { SprintDetailsPage } from "@/features/sprints/pages/SprintDetailsPage";
import { TasksPage } from "@/features/tasks/pages/TasksPage";
import { WorkspaceDetailsPage } from "@/features/workspace/pages/WorkspaceDetailsPage";
import { WorkspacesPage } from "@/features/workspace/pages/WorkspacesPage";

export function App() {
  return (
    <AppProviders>
      <Routes>
        <Route path={routes.login} element={<LoginPage />} />
        <Route path={routes.register} element={<RegisterPage />} />
        <Route path={routes.verifyEmail} element={<VerifyEmailPage />} />
        <Route path="/auth/callback/:provider" element={<OAuthCallbackPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppShell />}>
            <Route index element={<DashboardPage />} />
            <Route path="workspaces" element={<WorkspacesPage />} />
            <Route path="workspaces/:workspaceId" element={<WorkspaceDetailsPage />} />
            <Route path="workspaces/:workspaceId/projects" element={<ProjectsPage />} />
            <Route path="projects/:projectId" element={<ProjectDetailsPage />} />
            <Route path="projects/:projectId/board" element={<BoardPage />} />
            <Route path="sprints/:sprintId" element={<SprintDetailsPage />} />
            <Route path="tasks" element={<TasksPage />} />
            <Route path="account" element={<AccountPage />} />
            <Route path="settings" element={<Navigate to={routes.account} replace />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to={routes.dashboard} replace />} />
      </Routes>
    </AppProviders>
  );
}
