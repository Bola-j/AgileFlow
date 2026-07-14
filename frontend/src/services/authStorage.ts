import type { AuthResponseDto } from "@/types/api";

const STORAGE_KEY = "agileflow.auth";
const MEMORY_KEY = "agileflow.auth.memory";

export interface StoredAuth {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  role: string;
  remember: boolean;
}

let memoryAuth: StoredAuth | null = null;

export function toStoredAuth(response: AuthResponseDto, remember: boolean): StoredAuth {
  return { ...response, remember };
}

export function getStoredAuth(): StoredAuth | null {
  if (memoryAuth) return memoryAuth;
  const raw = localStorage.getItem(STORAGE_KEY) ?? sessionStorage.getItem(MEMORY_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as StoredAuth;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function setStoredAuth(auth: StoredAuth) {
  memoryAuth = auth;
  const target = auth.remember ? localStorage : sessionStorage;
  const other = auth.remember ? sessionStorage : localStorage;
  target.setItem(auth.remember ? STORAGE_KEY : MEMORY_KEY, JSON.stringify(auth));
  other.removeItem(auth.remember ? MEMORY_KEY : STORAGE_KEY);
}

export function clearStoredAuth() {
  memoryAuth = null;
  localStorage.removeItem(STORAGE_KEY);
  sessionStorage.removeItem(MEMORY_KEY);
}
