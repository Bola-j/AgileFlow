import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, Input } from "@/components/ui/forms";
import { ErrorState, Skeleton } from "@/components/ui/state";
import { accountApi } from "@/features/account/api/accountApi";
import { getErrorMessage } from "@/services/apiClient";
import { queryKeys } from "@/utils/queryKeys";

const schema = z.object({
  firstName: z.string().max(50).optional(),
  lastName: z.string().max(50).optional(),
  phoneNumber: z.string().optional(),
  profilePicture: z.string().url().optional().or(z.literal("")),
  dob: z.string().optional(),
  githubUsername: z.string().max(100).optional(),
});

type FormValues = z.infer<typeof schema>;

export function AccountPage() {
  const queryClient = useQueryClient();
  const account = useQuery({ queryKey: queryKeys.account, queryFn: accountApi.me });
  const form = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: {} });

  useEffect(() => {
    if (account.data) {
      form.reset({
        firstName: account.data.firstName,
        lastName: account.data.lastName,
        phoneNumber: account.data.phoneNumber ?? "",
        profilePicture: account.data.profilePicture ?? "",
        dob: account.data.dob ?? "",
        githubUsername: account.data.githubUsername ?? "",
      });
    }
  }, [account.data, form]);

  const update = useMutation({
    mutationFn: accountApi.updateMe,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.account });
      toast.success("Profile updated.");
    },
    onError: (error) => toast.error(getErrorMessage(error)),
  });

  if (account.isLoading) return <Skeleton className="h-96" />;
  if (account.isError) return <ErrorState onRetry={() => void account.refetch()} />;

  return (
    <>
      <PageHeader title="Account" description="View and update your AgileFlow profile." />
      <Card className="max-w-3xl">
        <CardHeader><CardTitle>Profile</CardTitle></CardHeader>
        <CardContent>
          <form className="grid gap-4" onSubmit={form.handleSubmit((values) => update.mutate(values))}>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="First name" error={form.formState.errors.firstName?.message}><Input {...form.register("firstName")} /></Field>
              <Field label="Last name" error={form.formState.errors.lastName?.message}><Input {...form.register("lastName")} /></Field>
            </div>
            <Field label="Email"><Input value={account.data?.email ?? ""} disabled /></Field>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Phone" error={form.formState.errors.phoneNumber?.message}><Input {...form.register("phoneNumber")} /></Field>
              <Field label="Date of birth" error={form.formState.errors.dob?.message}><Input type="date" {...form.register("dob")} /></Field>
            </div>
            <Field label="GitHub username" error={form.formState.errors.githubUsername?.message}><Input {...form.register("githubUsername")} /></Field>
            <Field label="Profile picture URL" error={form.formState.errors.profilePicture?.message}><Input {...form.register("profilePicture")} /></Field>
            <Button className="w-fit" type="submit" disabled={update.isPending}>{update.isPending ? "Saving..." : "Save profile"}</Button>
          </form>
        </CardContent>
      </Card>
    </>
  );
}
