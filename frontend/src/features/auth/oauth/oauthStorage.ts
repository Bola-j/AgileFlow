import type { OAuthProvider } from "@/features/auth/oauth/oauthConfig";

const OAUTH_STATE_KEY = "agileflow.oauth.state";

interface StoredOAuthState {
  provider: OAuthProvider;
  state: string;
  remember: boolean;
}

export function saveOAuthState(provider: OAuthProvider, state: string, remember: boolean) {
  const payload: StoredOAuthState = { provider, state, remember };
  sessionStorage.setItem(OAUTH_STATE_KEY, JSON.stringify(payload));
}

export function consumeOAuthState(provider: OAuthProvider, state: string): { remember: boolean } | null {
  const raw = sessionStorage.getItem(OAUTH_STATE_KEY);
  sessionStorage.removeItem(OAUTH_STATE_KEY);

  if (!raw) return null;

  try {
    const stored = JSON.parse(raw) as StoredOAuthState;
    if (stored.provider !== provider || stored.state !== state) return null;
    return { remember: stored.remember };
  } catch {
    return null;
  }
}

export function clearOAuthState() {
  sessionStorage.removeItem(OAUTH_STATE_KEY);
}
