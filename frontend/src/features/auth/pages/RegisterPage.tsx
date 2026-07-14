import { zodResolver } from "@hookform/resolvers/zod";
import { KanbanSquare } from "lucide-react";
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
  firstName: z.string().min(1).max(50),
  lastName: z.string().min(1).max(50),
  email: z.string().email(),
  password: z.string().min(8),
  remember: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

export function RegisterPage() {
  const { register } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { firstName: "", lastName: "", email: "", password: "", remember: true },
  });

  async function onSubmit(values: FormValues) {
    setError(null);
    try {
      await register({ firstName: values.firstName, lastName: values.lastName, email: values.email, password: values.password }, values.remember);
    } catch (submitError) {
      setError(getErrorMessage(submitError));
    }
  }

  return (
    <main className="grid min-h-screen place-items-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="mb-3 flex h-10 w-10 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <KanbanSquare className="h-5 w-5" />
          </div>
          <CardTitle>Create your AgileFlow account</CardTitle>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4" onSubmit={form.handleSubmit(onSubmit)}>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field label="First name" error={form.formState.errors.firstName?.message}><Input {...form.register("firstName")} /></Field>
              <Field label="Last name" error={form.formState.errors.lastName?.message}><Input {...form.register("lastName")} /></Field>
            </div>
            <Field label="Email" error={form.formState.errors.email?.message}><Input type="email" {...form.register("email")} /></Field>
            <Field label="Password" error={form.formState.errors.password?.message}><Input type="password" {...form.register("password")} /></Field>
            <label className="flex items-center gap-2 text-sm"><input type="checkbox" className="h-4 w-4 rounded border" {...form.register("remember")} />Remember me</label>
            {error ? <p className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">{error}</p> : null}
            <Button type="submit" disabled={form.formState.isSubmitting}>{form.formState.isSubmitting ? "Creating..." : "Create account"}</Button>
          </form>
          <p className="mt-5 text-sm text-muted-foreground">
            Already registered? <Link className="font-medium text-primary" to={routes.login}>Sign in</Link>
          </p>
        </CardContent>
      </Card>
    </main>
  );
}
