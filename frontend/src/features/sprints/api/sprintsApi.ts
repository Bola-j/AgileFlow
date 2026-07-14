import { apiClient } from "@/services/apiClient";
import type { CreateSprintRequest, SprintProgressResponse, SprintResponse, UpdateSprintRequest } from "@/types/api";

export const sprintsApi = {
  byProject: async (projectId: number) => (await apiClient.get<SprintResponse[]>(`/api/projects/${projectId}/sprints`)).data,
  get: async (id: number) => (await apiClient.get<SprintResponse>(`/api/sprints/${id}`)).data,
  create: async (projectId: number, payload: CreateSprintRequest) => (await apiClient.post<SprintResponse>(`/api/projects/${projectId}/sprints`, payload)).data,
  update: async (id: number, payload: UpdateSprintRequest) => (await apiClient.put<SprintResponse>(`/api/sprints/${id}`, payload)).data,
  start: async (id: number) => (await apiClient.put<SprintResponse>(`/api/sprints/${id}/start`)).data,
  complete: async (id: number) => (await apiClient.put<SprintResponse>(`/api/sprints/${id}/complete`)).data,
  progress: async (id: number) => (await apiClient.get<SprintProgressResponse>(`/api/sprints/${id}/progress`)).data,
};
