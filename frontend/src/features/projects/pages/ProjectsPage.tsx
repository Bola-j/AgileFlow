import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { SearchInput } from "@/components/shared/SearchInput";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmDialog, Modal } from "@/components/ui/dialog";
import { Field, Input, SelectInput, Textarea } from "@/components/ui/forms";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { routes } from "@/constants/routes";
import { accountApi } from "@/features/account/api/accountApi";
import { projectsApi } from "@/features/projects/api/projectsApi";
import { workspaceApi } from "@/features/workspace/api/workspaceApi";
import { getCurrentWorkspaceMember, isWorkspaceManager } from "@/features/workspace/utils/permissions";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { ProjectStatus } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

const schema = z.object({ name: z.string().min(1).max(100), description: z.string().max(500).optional(), status: z.number(), startDate: z.string().min(1), endDate: z.string().min(1) });

export function ProjectsPage() {
  const workspaceId = Number(useParams().workspaceId);
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<number | null>(null);
  const query = useQuery({ queryKey: queryKeys.projects(workspaceId), queryFn: () => projectsApi.byWorkspace(workspaceId), enabled: Number.isFinite(workspaceId) });
  const workspace = useQuery({ queryKey: queryKeys.workspace(workspaceId), queryFn: () => workspaceApi.get(workspaceId), enabled: Number.isFinite(workspaceId) });
  const account = useQuery({ queryKey: queryKeys.account, queryFn: accountApi.me });
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema), defaultValues: { name: "", description: "", status: ProjectStatus.InProgress, startDate: new Date().toISOString().slice(0, 10), endDate: "" } });
  const create = useMutation({ mutationFn: (values: z.infer<typeof schema>) => projectsApi.create({ ...values, status: values.status as 0 | 1 | 2 | 3, workspaceId }), onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.projects(workspaceId) }); setModalOpen(false); form.reset(); toast.success("Project created."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const remove = useMutation({ mutationFn: projectsApi.remove, onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.projects(workspaceId) }); setDeleteId(null); toast.success("Project deleted."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const projects = useMemo(() => (query.data ?? []).filter((project) => `${project.name} ${project.description} ${project.status}`.toLowerCase().includes(search.toLowerCase())), [query.data, search]);
  const currentMember = getCurrentWorkspaceMember(workspace.data, account.data);
  const canManageProjects = isWorkspaceManager(currentMember?.role);

  return (
    <>
      <PageHeader title="Projects" description="Plan and track delivery in this workspace." actions={canManageProjects ? <Button onClick={() => setModalOpen(true)}><Plus className="h-4 w-4" />Create project</Button> : null} />
      <div className="mb-5 max-w-md"><SearchInput value={search} onChange={setSearch} placeholder="Search projects" /></div>
      {query.isLoading ? <Skeleton className="h-80" /> : null}
      {query.isError ? <ErrorState onRetry={() => void query.refetch()} /> : null}
      {!query.isLoading && projects.length === 0 ? <EmptyState title="No projects found" /> : null}
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {projects.map((project) => <Card key={project.id}><CardHeader><CardTitle>{project.name}</CardTitle><CardDescription>{project.description || "No description"}</CardDescription></CardHeader><CardContent><div className="mb-4 flex flex-wrap gap-2"><Badge value={project.status} /><Badge value={`${formatDate(project.startDate)} - ${formatDate(project.endDate)}`} /></div><div className="flex justify-between gap-2"><Button asChild variant="outline"><Link to={routes.project(project.id)}>Open</Link></Button>{canManageProjects ? <Button size="icon" variant="ghost" onClick={() => setDeleteId(project.id)}><Trash2 className="h-4 w-4" /></Button> : null}</div></CardContent></Card>)}
      </div>
      <Modal open={modalOpen} onOpenChange={setModalOpen} title="Create project"><form className="grid gap-4" onSubmit={form.handleSubmit((values) => create.mutate(values))}><Field label="Name" error={form.formState.errors.name?.message}><Input {...form.register("name")} /></Field><Field label="Description"><Textarea {...form.register("description")} /></Field><div className="grid gap-4 sm:grid-cols-3"><Field label="Status"><SelectInput {...form.register("status", { valueAsNumber: true })}><option value={ProjectStatus.InProgress}>In progress</option><option value={ProjectStatus.Completed}>Completed</option><option value={ProjectStatus.OnHold}>On hold</option><option value={ProjectStatus.Cancelled}>Cancelled</option></SelectInput></Field><Field label="Start"><Input type="date" {...form.register("startDate")} /></Field><Field label="End"><Input type="date" {...form.register("endDate")} /></Field></div><Button disabled={create.isPending}>{create.isPending ? "Creating..." : "Create"}</Button></form></Modal>
      <ConfirmDialog open={deleteId !== null} onOpenChange={(open) => !open && setDeleteId(null)} title="Delete project" description="This removes the project if authorized." busy={remove.isPending} onConfirm={() => deleteId && remove.mutate(deleteId)} />
    </>
  );
}
