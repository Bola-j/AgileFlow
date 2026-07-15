import { apiClient } from "@/services/apiClient";
import type { DashboardSummaryResponse, MyTaskResponse } from "@/types/api";

export const dashboardApi = {
  summary: async () => (await apiClient.get<DashboardSummaryResponse>("/api/dashboard/summary")).data,
  myTasks: async () => (await apiClient.get<MyTaskResponse[]>("/api/dashboard/my-tasks")).data,
};
