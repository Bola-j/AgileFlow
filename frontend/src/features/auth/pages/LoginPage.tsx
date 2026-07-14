import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, KanbanSquare } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, Input } from "@/components/ui/forms";
import { routes } from "@/constants/routes";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getErrorMessage } from "@/services/apiClient";

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(1, "Password is required"),
  remember: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const { login } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const form = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { email: "", password: "", remember: true } });

  async function onSubmit(values: FormValues) {
    setError(null);
    try {
      await login({ email: values.email, password: values.password }, values.remember);
    } catch (submitError) {
      setError(getErrorMessage(submitError));
    }
  }

  return (
    <AuthFrame>
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <KanbanSquare className="h-5 w-5" />
          </div>
          <CardTitle>Sign in to AgileFlow</CardTitle>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4" onSubmit={form.handleSubmit(onSubmit)}>
            <Field label="Email" error={form.formState.errors.email?.message}>
              <Input type="email" autoComplete="email" {...form.register("email")} />
            </Field>
            <Field label="Password" error={form.formState.errors.password?.message}>
              <div className="relative">
                <Input type={showPassword ? "text" : "password"} autoComplete="current-password" {...form.register("password")} />
                <button type="button" className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground" onClick={() => setShowPassword((value) => !value)} aria-label="Toggle password">
                  <Eye className="h-4 w-4" />
                </button>
              </div>
            </Field>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" className="h-4 w-4 rounded border" {...form.register("remember")} />
              Remember me
            </label>
            {error ? <p className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">{error}</p> : null}
            <Button type="submit" disabled={form.formState.isSubmitting}>{form.formState.isSubmitting ? "Signing in..." : "Sign in"}</Button>
          </form>
          <p className="mt-5 text-sm text-muted-foreground">
            No account? <Link className="font-medium text-primary" to={routes.register}>Create one</Link>
          </p>
        </CardContent>
      </Card>
    </AuthFrame>
  );
}

function AuthFrame({ children }: { children: React.ReactNode }) {
  return <main className="grid min-h-screen place-items-center bg-background p-4">{children}</main>;
}
