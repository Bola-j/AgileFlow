import { apiClient } from "@/services/apiClient";
import type {
  AddWorkspaceMemberRequest,
  CreateWorkspaceRequest,
  UpdateMemberProfileByAdminRequest,
  UpdateWorkspaceMemberRoleRequest,
  UpdateWorkspaceRequest,
  WorkspaceMemberDetailResponse,
  WorkspaceResponse,
  WorkspaceSummaryResponse,
} from "@/types/api";

export const workspaceApi = {
  list: async () => (await apiClient.get<WorkspaceSummaryResponse[]>("/api/Workspaces")).data,
  get: async (id: number) => (await apiClient.get<WorkspaceResponse>(`/api/Workspaces/${id}`)).data,
  create: async (payload: CreateWorkspaceRequest) => (await apiClient.post<WorkspaceResponse>("/api/Workspaces", payload)).data,
  update: async (id: number, payload: UpdateWorkspaceRequest) => (await apiClient.put<WorkspaceResponse>(`/api/Workspaces/${id}`, payload)).data,
  remove: async (id: number) => apiClient.delete(`/api/Workspaces/${id}`),
  addMember: async (workspaceId: number, payload: AddWorkspaceMemberRequest) => apiClient.post(`/api/Workspaces/${workspaceId}/members`, payload),
  updateMemberRole: async (workspaceId: number, memberUserId: string, payload: UpdateWorkspaceMemberRoleRequest) =>
    apiClient.put(`/api/Workspaces/${workspaceId}/members/${memberUserId}/role`, payload),
  removeMember: async (workspaceId: number, memberEmail: string) => apiClient.delete(`/api/Workspaces/${workspaceId}/members/${encodeURIComponent(memberEmail)}`),
  updateMemberProfile: async (workspaceId: number, memberUserId: string, payload: UpdateMemberProfileByAdminRequest) =>
    apiClient.put(`/api/Workspaces/${workspaceId}/members/${memberUserId}`, payload),
  getMember: async (workspaceId: number, memberUserId: string) =>
    (await apiClient.get<WorkspaceMemberDetailResponse>(`/api/Workspaces/${workspaceId}/members/${memberUserId}`)).data,
};
