import { useEffect, useRef, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { routes } from "@/constants/routes";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { oauthConfig, type OAuthProvider } from "@/features/auth/oauth/oauthConfig";
import { clearOAuthState, consumeOAuthState } from "@/features/auth/oauth/oauthStorage";
import { getErrorMessage } from "@/services/apiClient";

const providerLabels: Record<OAuthProvider, string> = {
  google: "Google",
  github: "GitHub",
};

function isOAuthProvider(value: string | undefined): value is OAuthProvider {
  return value === "google" || value === "github";
}

export function OAuthCallbackPage() {
  const { provider: providerParam } = useParams();
  const [searchParams] = useSearchParams();
  const { loginWithOAuth } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const hasStarted = useRef(false);

  useEffect(() => {
    // React StrictMode intentionally runs effects twice in development. The code can only
    // be exchanged once, so prevent a second effect from consuming the saved OAuth state.
    if (hasStarted.current) return;
    hasStarted.current = true;

    if (!isOAuthProvider(providerParam)) {
      setError("Unsupported OAuth provider.");
      return;
    }

    const provider = providerParam;
    const oauthError = searchParams.get("error");
    const code = searchParams.get("code");
    const state = searchParams.get("state");

    if (oauthError) {
      clearOAuthState();
      setError(oauthError === "access_denied" ? "Sign-in was cancelled." : "OAuth sign-in failed.");
      return;
    }

    if (!code || !state) {
      clearOAuthState();
      setError("Missing OAuth authorization response.");
      return;
    }

    const authorizationCode = code;

    const storedState = consumeOAuthState(provider, state);
    if (!storedState) {
      setError("Invalid OAuth state. Please try signing in again.");
      return;
    }

    const remember = storedState.remember;

    async function completeOAuth() {
      try {
        await loginWithOAuth(
          provider,
          {
            code: authorizationCode,
            redirectUri: oauthConfig.redirectUri(provider),
          },
          remember,
        );
      } catch (callbackError) {
        setError(getErrorMessage(callbackError));
      }
    }

    void completeOAuth();
  }, [loginWithOAuth, providerParam, searchParams]);

  const providerName = isOAuthProvider(providerParam) ? providerLabels[providerParam] : "OAuth";

  return (
    <main className="grid min-h-screen place-items-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>{providerName} sign-in</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {error ? (
            <>
              <p className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">{error}</p>
              <Button asChild className="w-full">
                <Link to={routes.login}>Back to sign in</Link>
              </Button>
            </>
          ) : (
            <p className="text-sm text-muted-foreground">Completing sign-in...</p>
          )}
        </CardContent>
      </Card>
    </main>
  );
}
