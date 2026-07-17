export type OAuthProvider = "google" | "github";

const frontendOrigin = typeof window !== "undefined" ? window.location.origin : "http://127.0.0.1:5173";

export const oauthConfig = {
  google: {
    clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "",
    authorizeUrl: "https://accounts.google.com/o/oauth2/v2/auth",
    scope: "openid email profile",
  },
  github: {
    clientId: import.meta.env.VITE_GITHUB_CLIENT_ID ?? "",
    authorizeUrl: "https://github.com/login/oauth/authorize",
    scope: "user:email",
  },
  redirectUri: (provider: OAuthProvider) => `${frontendOrigin}/auth/callback/${provider}`,
} as const;

export function isOAuthProviderConfigured(provider: OAuthProvider) {
  return provider === "google"
    ? Boolean(oauthConfig.google.clientId)
    : Boolean(oauthConfig.github.clientId);
}

export function buildOAuthAuthorizationUrl(provider: OAuthProvider, state: string) {
  const config = oauthConfig[provider];
  const params = new URLSearchParams({
    client_id: config.clientId,
    redirect_uri: oauthConfig.redirectUri(provider),
    response_type: "code",
    scope: config.scope,
    state,
  });

  if (provider === "google") {
    params.set("access_type", "online");
    params.set("prompt", "select_account");
  }

  return `${config.authorizeUrl}?${params.toString()}`;
}
