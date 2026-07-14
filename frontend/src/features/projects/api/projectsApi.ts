import { apiClient } from "@/services/apiClient";
import type { CreateProjectRequest, ProjectResponse, UpdateProjectRequest } from "@/types/api";

export const projectsApi = {
  byWorkspace: async (workspaceId: number) => (await apiClient.get<ProjectResponse[]>(`/api/Projects/workspace/${workspaceId}`)).data,
  get: async (id: number) => (await apiClient.get<ProjectResponse>(`/api/Projects/${id}`)).data,
  create: async (payload: CreateProjectRequest) => (await apiClient.post<ProjectResponse>("/api/Projects", payload)).data,
  update: async (id: number, payload: UpdateProjectRequest) => (await apiClient.put<ProjectResponse>(`/api/Projects/${id}`, payload)).data,
  remove: async (id: number) => apiClient.delete(`/api/Projects/${id}`),
};
