import { apiClient } from "@/services/apiClient";
import type { AccountResponse, UpdateAccountRequest } from "@/types/api";

export const accountApi = {
  me: async () => {
    const { data } = await apiClient.get<AccountResponse>("/api/account/me");
    return data;
  },
  updateMe: async (payload: UpdateAccountRequest) => {
    const { data } = await apiClient.put<AccountResponse>("/api/account/me", payload);
    return data;
  },
  uploadProfilePicture: async (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    const { data } = await apiClient.post<AccountResponse>("/api/account/me/profile-picture", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return data;
  },
};
