import { useEffect, useState } from "react";
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

  useEffect(() => {
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

    const storedState = consumeOAuthState(provider, state);
    if (!storedState) {
      setError("Invalid OAuth state. Please try signing in again.");
      return;
    }

    let cancelled = false;

    async function completeOAuth() {
      try {
        await loginWithOAuth(
          provider,
          {
            code,
            redirectUri: oauthConfig.redirectUri(provider),
          },
          storedState!.remember,
        );
      } catch (callbackError) {
        if (!cancelled) setError(getErrorMessage(callbackError));
      }
    }

    void completeOAuth();

    return () => {
      cancelled = true;
    };
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
