import { apiClient } from "@/services/apiClient";
import type { AuthResponseDto, LoginRequestDto, LogoutRequestDto, RegisterRequestDto } from "@/types/api";

export const authApi = {
  register: async (payload: RegisterRequestDto) => {
    const { data } = await apiClient.post<AuthResponseDto>("/api/auth/register", payload);
    return data;
  },
  login: async (payload: LoginRequestDto) => {
    const { data } = await apiClient.post<AuthResponseDto>("/api/auth/login", payload);
    return data;
  },
  logout: async (payload: LogoutRequestDto) => {
    await apiClient.post("/api/auth/logout", payload);
  },
};
