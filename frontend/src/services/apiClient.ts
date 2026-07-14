import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { clearStoredAuth, getStoredAuth, setStoredAuth, toStoredAuth } from "@/services/authStorage";
import type { AuthResponseDto, RefreshRequestDto } from "@/types/api";

const baseURL = import.meta.env.VITE_API_URL ?? "http://localhost:6358";

interface RetryConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

export const apiClient = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const auth = getStoredAuth();
  if (auth?.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetryConfig | undefined;
    const auth = getStoredAuth();

    if (error.response?.status === 401 && original && !original._retry && auth?.refreshToken && auth?.accessToken) {
      original._retry = true;
      try {
        const payload: RefreshRequestDto = { accessToken: auth.accessToken, refreshToken: auth.refreshToken };
        const response = await axios.post<AuthResponseDto>(`${baseURL}/api/auth/refresh`, payload);
        const refreshed = toStoredAuth(response.data, auth.remember);
        setStoredAuth(refreshed);
        original.headers.Authorization = `Bearer ${refreshed.accessToken}`;
        return apiClient(original);
      } catch {
        clearStoredAuth();
        window.dispatchEvent(new Event("agileflow:auth-expired"));
      }
    }

    return Promise.reject(error);
  },
);

export function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data === "string") return data;
    if (data && typeof data === "object" && "message" in data && typeof data.message === "string") return data.message;
    if (error.response?.status === 403) return "You do not have permission to perform this action.";
    if (error.response?.status === 404) return "The requested resource was not found.";
    if (error.response?.status === 409) return "The request conflicts with the current state.";
    if (error.response?.status === 500) return "The server could not complete the request.";
  }
  return "Something went wrong. Please try again.";
}
