import { apiClient } from "@/services/apiClient";
import type { AddColumnRequest, GetBoardDetailsResponse, UpdateColumnOrderRequest, UpdateColumnRequest } from "@/types/api";

export const boardApi = {
  get: async (projectId: number, sprintId: number) => (await apiClient.get<GetBoardDetailsResponse>(`/api/projects/${projectId}/board`, { params: { sprintId } })).data,
  addColumn: async (projectId: number, payload: AddColumnRequest) => apiClient.post(`/api/projects/${projectId}/board/columns`, payload),
  updateColumn: async (columnId: number, payload: UpdateColumnRequest) => apiClient.put(`/api/columns/${columnId}`, payload),
  deleteColumn: async (columnId: number) => apiClient.delete(`/api/columns/${columnId}`),
  updateOrder: async (projectId: number, payload: UpdateColumnOrderRequest) => apiClient.put(`/api/projects/${projectId}/board/columns/order`, payload),
};
