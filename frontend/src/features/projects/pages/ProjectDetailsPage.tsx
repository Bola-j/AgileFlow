import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Activity, Calendar, KanbanSquare, Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Modal } from "@/components/ui/dialog";
import { Field, Input, SelectInput, Textarea } from "@/components/ui/forms";
import { ErrorState, Skeleton } from "@/components/ui/state";
import { routes } from "@/constants/routes";
import { projectsApi } from "@/features/projects/api/projectsApi";
import { sprintsApi } from "@/features/sprints/api/sprintsApi";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { ProjectStatus } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

const projectSchema = z.object({ name: z.string().min(1).max(100), description: z.string().max(500).optional(), status: z.number(), endDate: z.string().min(1) });
const sprintSchema = z.object({ name: z.string().min(1).max(100), goal: z.string().min(1).max(500), startDate: z.string().min(1), endDate: z.string().min(1) });

export function ProjectDetailsPage() {
  const projectId = Number(useParams().projectId);
  const queryClient = useQueryClient();
  const [projectOpen, setProjectOpen] = useState(false);
  const [sprintOpen, setSprintOpen] = useState(false);
  const project = useQuery({ queryKey: queryKeys.project(projectId), queryFn: () => projectsApi.get(projectId), enabled: Number.isFinite(projectId) });
  const sprints = useQuery({ queryKey: queryKeys.sprints(projectId), queryFn: () => sprintsApi.byProject(projectId), enabled: Number.isFinite(projectId) });
  const projectForm = useForm<z.infer<typeof projectSchema>>({ resolver: zodResolver(projectSchema), values: { name: project.data?.name ?? "", description: project.data?.description ?? "", status: statusValue(project.data?.status), endDate: project.data?.endDate?.slice(0, 10) ?? "" } });
  const sprintForm = useForm<z.infer<typeof sprintSchema>>({ resolver: zodResolver(sprintSchema), defaultValues: { name: "", goal: "", startDate: new Date().toISOString().slice(0, 10), endDate: "" } });
  const updateProject = useMutation({ mutationFn: (values: z.infer<typeof projectSchema>) => projectsApi.update(projectId, { ...values, status: values.status as 0 | 1 | 2 | 3 }), onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.project(projectId) }); setProjectOpen(false); toast.success("Project updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const createSprint = useMutation({ mutationFn: (values: z.infer<typeof sprintSchema>) => sprintsApi.create(projectId, values), onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.sprints(projectId) }); setSprintOpen(false); sprintForm.reset(); toast.success("Sprint created."); }, onError: (error) => toast.error(getErrorMessage(error)) });

  if (project.isLoading) return <Skeleton className="h-96" />;
  if (project.isError || !project.data) return <ErrorState onRetry={() => void project.refetch()} />;

  const activeSprint = sprints.data?.find((sprint) => sprint.status === "Active") ?? sprints.data?.[0];

  return (
    <>
      <PageHeader title={project.data.name} description={project.data.description || "Project details"} actions={<><Button variant="outline" onClick={() => setProjectOpen(true)}><Pencil className="h-4 w-4" />Edit</Button>{activeSprint ? <Button asChild><Link to={routes.sprint(activeSprint.id)}><KanbanSquare className="h-4 w-4" />Active sprint</Link></Button> : null}<Button onClick={() => setSprintOpen(true)}><Plus className="h-4 w-4" />Sprint</Button></>} />
      <div className="mb-6 grid gap-4 md:grid-cols-3">
        <Card><CardHeader><CardTitle>Status</CardTitle></CardHeader><CardContent><Badge value={project.data.status} /></CardContent></Card>
        <Card><CardHeader><CardTitle>Timeline</CardTitle></CardHeader><CardContent className="flex items-center gap-2 text-sm"><Calendar className="h-4 w-4" />{formatDate(project.data.startDate)} - {formatDate(project.data.endDate)}</CardContent></Card>
        <Card><CardHeader><CardTitle>Sprints</CardTitle></CardHeader><CardContent className="flex items-center gap-2 text-sm"><Activity className="h-4 w-4" />{sprints.data?.length ?? 0} total</CardContent></Card>
      </div>
      <Card>
        <CardHeader><CardTitle>Sprints</CardTitle><CardDescription>Use active sprints to open the Kanban board and task backlog.</CardDescription></CardHeader>
        <CardContent className="grid gap-3">
          {sprints.isLoading ? <Skeleton className="h-32" /> : null}
          {(sprints.data ?? []).map((sprint) => <Link key={sprint.id} to={routes.sprint(sprint.id)} className="flex flex-col gap-2 rounded-md border p-4 transition hover:bg-muted sm:flex-row sm:items-center sm:justify-between"><div><p className="font-medium">{sprint.name}</p><p className="text-sm text-muted-foreground">{sprint.goal}</p></div><div className="flex gap-2"><Badge value={sprint.status} /><Badge value={`${sprint.taskCount} tasks`} /></div></Link>)}
        </CardContent>
      </Card>
      <Modal open={projectOpen} onOpenChange={setProjectOpen} title="Edit project"><form className="grid gap-4" onSubmit={projectForm.handleSubmit((values) => {
        if (new Date(values.endDate) <= new Date(project.data.startDate.slice(0, 10))) {
          projectForm.setError("endDate", { message: "End date must be after the project start date." });
          return;
        }
        updateProject.mutate(values);
      })}><Field label="Name" error={projectForm.formState.errors.name?.message}><Input {...projectForm.register("name")} /></Field><Field label="Description" error={projectForm.formState.errors.description?.message}><Textarea {...projectForm.register("description")} /></Field><div className="grid gap-4 sm:grid-cols-2"><Field label="Status"><SelectInput {...projectForm.register("status", { valueAsNumber: true })}><option value={ProjectStatus.InProgress}>In progress</option><option value={ProjectStatus.Completed}>Completed</option><option value={ProjectStatus.OnHold}>On hold</option><option value={ProjectStatus.Cancelled}>Cancelled</option></SelectInput></Field><Field label="End date" error={projectForm.formState.errors.endDate?.message}><Input type="date" {...projectForm.register("endDate")} /></Field></div><Button disabled={updateProject.isPending}>{updateProject.isPending ? "Saving..." : "Save"}</Button></form></Modal>
      <Modal open={sprintOpen} onOpenChange={setSprintOpen} title="Create sprint"><form className="grid gap-4" onSubmit={sprintForm.handleSubmit((values) => {
        const projectStart = new Date(project.data.startDate.slice(0, 10));
        const projectEnd = new Date(project.data.endDate.slice(0, 10));
        const sprintStart = new Date(values.startDate);
        const sprintEnd = new Date(values.endDate);
        if (sprintStart < projectStart) {
          sprintForm.setError("startDate", { message: "Sprint start date cannot be before the project start date." });
          return;
        }
        if (sprintEnd <= sprintStart) {
          sprintForm.setError("endDate", { message: "End date must be after start date." });
          return;
        }
        if (sprintEnd > projectEnd) {
          sprintForm.setError("endDate", { message: "Sprint end date cannot be after the project end date." });
          return;
        }
        createSprint.mutate(values);
      })}><Field label="Name" error={sprintForm.formState.errors.name?.message}><Input {...sprintForm.register("name")} /></Field><Field label="Goal" error={sprintForm.formState.errors.goal?.message}><Textarea {...sprintForm.register("goal")} /></Field><div className="grid gap-4 sm:grid-cols-2"><Field label="Start date" error={sprintForm.formState.errors.startDate?.message}><Input type="date" {...sprintForm.register("startDate")} /></Field><Field label="End date" error={sprintForm.formState.errors.endDate?.message}><Input type="date" {...sprintForm.register("endDate")} /></Field></div><Button disabled={createSprint.isPending}>{createSprint.isPending ? "Creating..." : "Create"}</Button></form></Modal>
    </>
  );
}

function statusValue(status?: string): 0 | 1 | 2 | 3 {
  if (status === "Completed") return ProjectStatus.Completed;
  if (status === "OnHold") return ProjectStatus.OnHold;
  if (status === "Cancelled") return ProjectStatus.Cancelled;
  return ProjectStatus.InProgress;
}
