import { apiClient } from "@/services/apiClient";
import type {
  AssignTaskRequest,
  CreateTaskRequest,
  MoveTaskRequest,
  TaskActivityLogResponse,
  TaskDetailResponse,
  TaskSummaryResponse,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
} from "@/types/api";

export const tasksApi = {
  bySprint: async (sprintId: number) => (await apiClient.get<TaskSummaryResponse[]>(`/api/sprints/${sprintId}/tasks`)).data,
  get: async (id: number) => (await apiClient.get<TaskDetailResponse>(`/api/tasks/${id}`)).data,
  create: async (sprintId: number, payload: CreateTaskRequest) => (await apiClient.post<TaskDetailResponse>(`/api/sprints/${sprintId}/tasks`, payload)).data,
  update: async (id: number, payload: UpdateTaskRequest) => (await apiClient.put<TaskDetailResponse>(`/api/tasks/${id}`, payload)).data,
  updateStatus: async (id: number, payload: UpdateTaskStatusRequest) => (await apiClient.patch<TaskDetailResponse>(`/api/tasks/${id}/status`, payload)).data,
  move: async (id: number, payload: MoveTaskRequest) => (await apiClient.put<TaskDetailResponse>(`/api/tasks/${id}/move`, payload)).data,
  assign: async (id: number, payload: AssignTaskRequest) => (await apiClient.post<TaskDetailResponse>(`/api/tasks/${id}/assignees`, payload)).data,
  unassign: async (id: number, userId: string) => (await apiClient.delete<TaskDetailResponse>(`/api/tasks/${id}/assignees/${userId}`)).data,
  remove: async (id: number) => apiClient.delete(`/api/tasks/${id}`),
  addDependency: async (id: number, dependencyTaskId: number) => apiClient.post(`/api/tasks/${id}/dependencies/${dependencyTaskId}`),
  removeDependency: async (id: number, dependencyTaskId: number) => apiClient.delete(`/api/tasks/${id}/dependencies/${dependencyTaskId}`),
  activity: async (id: number) => (await apiClient.get<TaskActivityLogResponse[]>(`/api/tasks/${id}/activity-logs`)).data,
};
