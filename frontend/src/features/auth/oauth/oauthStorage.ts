import type { OAuthProvider } from "@/features/auth/oauth/oauthConfig";

const OAUTH_STATE_KEY = "agileflow.oauth.state";
const OAUTH_STATE_TTL_MS = 10 * 60 * 1000;

interface StoredOAuthState {
  provider: OAuthProvider;
  state: string;
  remember: boolean;
  createdAt: number;
}

export function saveOAuthState(provider: OAuthProvider, state: string, remember: boolean) {
  const payload: StoredOAuthState = { provider, state, remember, createdAt: Date.now() };
  sessionStorage.setItem(OAUTH_STATE_KEY, JSON.stringify(payload));
}

export function consumeOAuthState(provider: OAuthProvider, state: string): { remember: boolean } | null {
  const raw = sessionStorage.getItem(OAUTH_STATE_KEY);
  sessionStorage.removeItem(OAUTH_STATE_KEY);

  if (!raw) return null;

  try {
    const stored = JSON.parse(raw) as StoredOAuthState;
    if (stored.provider !== provider || stored.state !== state) return null;
    if (!Number.isFinite(stored.createdAt) || stored.createdAt > Date.now() || Date.now() - stored.createdAt > OAUTH_STATE_TTL_MS) {
      return null;
    }
    return { remember: stored.remember };
  } catch {
    return null;
  }
}

export function clearOAuthState() {
  sessionStorage.removeItem(OAUTH_STATE_KEY);
}
