import { CheckCircle2, KanbanSquare, XCircle } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { routes } from "@/constants/routes";
import { authApi } from "@/features/auth/api/authApi";
import { getErrorMessage } from "@/services/apiClient";

type VerifyState = "loading" | "success" | "error";

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const [state, setState] = useState<VerifyState>("loading");
  const [message, setMessage] = useState("Verifying your email...");
  const [email, setEmail] = useState<string | null>(null);
  const [isResending, setIsResending] = useState(false);

  useEffect(() => {
    const userId = searchParams.get("userId");
    const token = searchParams.get("token");

    if (!userId || !token) {
      setState("error");
      setMessage("Verification link is missing required information.");
      return;
    }

    let cancelled = false;
    authApi.confirmEmail(userId, token)
      .then((response) => {
        if (cancelled) return;
        setEmail(response.email);
        setState(response.confirmed ? "success" : "error");
        setMessage(response.message);
      })
      .catch((error) => {
        if (cancelled) return;
        setState("error");
        setMessage(getErrorMessage(error));
      });

    return () => {
      cancelled = true;
    };
  }, [searchParams]);

  async function resendVerification() {
    if (!email) return;
    setIsResending(true);
    try {
      await authApi.resendConfirmation({ email });
      setMessage("Verification email sent. Check your inbox.");
    } catch (error) {
      setMessage(getErrorMessage(error));
    } finally {
      setIsResending(false);
    }
  }

  const isSuccess = state === "success";

  return (
    <main className="grid min-h-screen place-items-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <KanbanSquare className="h-5 w-5" />
          </div>
          <CardTitle>Email verification</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4">
          <div className="flex items-start gap-3 rounded-md border p-3">
            {isSuccess ? <CheckCircle2 className="mt-0.5 h-5 w-5 text-emerald-600" /> : <XCircle className="mt-0.5 h-5 w-5 text-destructive" />}
            <p className="text-sm text-muted-foreground">{message}</p>
          </div>
          <div className="flex flex-col gap-2 sm:flex-row">
            <Button asChild><Link to={routes.login}>Go to sign in</Link></Button>
            {!isSuccess && email ? (
              <Button type="button" variant="outline" disabled={isResending} onClick={resendVerification}>
                {isResending ? "Sending..." : "Resend email"}
              </Button>
            ) : null}
          </div>
        </CardContent>
      </Card>
    </main>
  );
}
