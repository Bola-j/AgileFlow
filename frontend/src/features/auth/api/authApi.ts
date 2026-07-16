import { apiClient } from "@/services/apiClient";
import type {
  AuthResponseDto,
  ConfirmEmailResponseDto,
  LoginRequestDto,
  LogoutRequestDto,
  RegisterRequestDto,
  RegisterResponseDto,
  ResendEmailConfirmationRequestDto,
} from "@/types/api";

export const authApi = {
  register: async (payload: RegisterRequestDto) => {
    const { data } = await apiClient.post<RegisterResponseDto>("/api/auth/register", payload);
    return data;
  },
  confirmEmail: async (userId: string, token: string) => {
    const { data } = await apiClient.get<ConfirmEmailResponseDto>("/api/auth/confirm-email", {
      params: { userId, token },
    });
    return data;
  },
  resendConfirmation: async (payload: ResendEmailConfirmationRequestDto) => {
    await apiClient.post("/api/auth/resend-confirmation", payload);
  },
  login: async (payload: LoginRequestDto) => {
    const { data } = await apiClient.post<AuthResponseDto>("/api/auth/login", payload);
    return data;
  },
  logout: async (payload: LogoutRequestDto) => {
    await apiClient.post("/api/auth/logout", payload);
  },
};
