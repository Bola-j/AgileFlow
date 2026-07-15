import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { UserAvatar } from "@/components/shared/UserAvatar";
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
  const uploadPicture = useMutation({
    mutationFn: accountApi.uploadProfilePicture,
    onSuccess: async (updatedAccount) => {
      queryClient.setQueryData(queryKeys.account, updatedAccount);
      await queryClient.invalidateQueries({ queryKey: queryKeys.account });
      toast.success("Profile photo updated.");
    },
    onError: (error) => toast.error(getErrorMessage(error)),
  });

  if (account.isLoading) return <Skeleton className="h-96" />;
  if (account.isError) return <ErrorState onRetry={() => void account.refetch()} />;

  return (
    <>
      <PageHeader title="Account" description="View and update your AgileFlow profile." />
      <Card className="max-w-3xl">
        <CardHeader>
          <div className="flex items-center gap-4">
            <UserAvatar
              className="h-16 w-16 text-lg"
              src={account.data?.profilePicture}
              name={`${account.data?.firstName ?? ""} ${account.data?.lastName ?? ""}`.trim()}
              email={account.data?.email}
            />
            <div>
              <CardTitle>Profile</CardTitle>
              <p className="mt-1 text-sm text-muted-foreground">{account.data?.email}</p>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="mb-6 max-w-md">
            <Field label="Upload profile photo">
              <Input
                type="file"
                accept="image/png,image/jpeg,image/webp,image/gif"
                disabled={uploadPicture.isPending}
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (!file) return;
                  uploadPicture.mutate(file);
                  event.target.value = "";
                }}
              />
            </Field>
            <p className="mt-2 text-xs text-muted-foreground">{uploadPicture.isPending ? "Uploading..." : "JPG, PNG, WEBP, or GIF up to 5 MB."}</p>
          </div>
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
            <Button className="w-fit" type="submit" disabled={update.isPending}>{update.isPending ? "Saving..." : "Save profile"}</Button>
          </form>
        </CardContent>
      </Card>
    </>
  );
}
