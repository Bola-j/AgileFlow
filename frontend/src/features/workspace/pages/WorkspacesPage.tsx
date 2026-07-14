import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { SearchInput } from "@/components/shared/SearchInput";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmDialog, Modal } from "@/components/ui/dialog";
import { Field, Input, Textarea } from "@/components/ui/forms";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { routes } from "@/constants/routes";
import { workspaceApi } from "@/features/workspace/api/workspaceApi";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { queryKeys } from "@/utils/queryKeys";

const schema = z.object({ name: z.string().min(1).max(100), description: z.string().max(500).optional() });
type FormValues = z.infer<typeof schema>;

export function WorkspacesPage() {
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const queryClient = useQueryClient();
  const query = useQuery({ queryKey: queryKeys.workspaces, queryFn: workspaceApi.list });
  const form = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { name: "", description: "" } });

  const create = useMutation({
    mutationFn: workspaceApi.create,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.workspaces });
      setModalOpen(false);
      form.reset();
      toast.success("Workspace created.");
    },
    onError: (error) => toast.error(getErrorMessage(error)),
  });
  const remove = useMutation({
    mutationFn: workspaceApi.remove,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.workspaces });
      setDeleteId(null);
      toast.success("Workspace deleted.");
    },
    onError: (error) => toast.error(getErrorMessage(error)),
  });

  const workspaces = (query.data ?? []).filter((workspace) => `${workspace.name} ${workspace.description}`.toLowerCase().includes(search.toLowerCase()));

  return (
    <>
      <PageHeader title="Workspaces" description="Manage the spaces your teams use to organize delivery." actions={<Button onClick={() => setModalOpen(true)}><Plus className="h-4 w-4" />Create workspace</Button>} />
      <div className="mb-5 max-w-md"><SearchInput value={search} onChange={setSearch} placeholder="Search workspaces" /></div>
      {query.isLoading ? <div className="grid gap-4 md:grid-cols-3"><Skeleton className="h-44" /><Skeleton className="h-44" /><Skeleton className="h-44" /></div> : null}
      {query.isError ? <ErrorState onRetry={() => void query.refetch()} /> : null}
      {!query.isLoading && !query.isError && workspaces.length === 0 ? <EmptyState title="No workspaces found" description="Create a workspace to start adding projects and team members." /> : null}
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {workspaces.map((workspace) => (
          <Card key={workspace.id}>
            <CardHeader>
              <CardTitle>{workspace.name}</CardTitle>
              <CardDescription>{workspace.description || "No description"}</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="mb-4 flex flex-wrap gap-2">
                <Badge value={`${workspace.projectCount} projects`} />
                <Badge value={`${workspace.memberCount} members`} />
                <Badge value={formatDate(workspace.createdAt)} />
              </div>
              <div className="flex justify-between gap-2">
                <Button asChild variant="outline"><Link to={routes.workspace(workspace.id)}>Open</Link></Button>
                <Button size="icon" variant="ghost" onClick={() => setDeleteId(workspace.id)} aria-label="Delete workspace"><Trash2 className="h-4 w-4" /></Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
      <Modal open={modalOpen} onOpenChange={setModalOpen} title="Create workspace">
        <form className="grid gap-4" onSubmit={form.handleSubmit((values) => create.mutate(values))}>
          <Field label="Name" error={form.formState.errors.name?.message}><Input {...form.register("name")} /></Field>
          <Field label="Description" error={form.formState.errors.description?.message}><Textarea {...form.register("description")} /></Field>
          <Button type="submit" disabled={create.isPending}>{create.isPending ? "Creating..." : "Create"}</Button>
        </form>
      </Modal>
      <ConfirmDialog open={deleteId !== null} onOpenChange={(open) => !open && setDeleteId(null)} title="Delete workspace" description="This removes the workspace if the backend authorizes the action." busy={remove.isPending} onConfirm={() => deleteId && remove.mutate(deleteId)} />
    </>
  );
}
