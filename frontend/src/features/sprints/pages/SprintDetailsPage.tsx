import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, Pencil, PlayCircle } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useParams } from "react-router-dom";
import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Modal } from "@/components/ui/dialog";
import { Field, Input, Textarea } from "@/components/ui/forms";
import { ErrorState, Skeleton } from "@/components/ui/state";
import { routes } from "@/constants/routes";
import { sprintsApi } from "@/features/sprints/api/sprintsApi";
import { tasksApi } from "@/features/tasks/api/tasksApi";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { queryKeys } from "@/utils/queryKeys";

const schema = z.object({ name: z.string().min(1).max(100), goal: z.string().min(1).max(500), endDate: z.string().min(1) });

export function SprintDetailsPage() {
  const sprintId = Number(useParams().sprintId);
  const queryClient = useQueryClient();
  const [editOpen, setEditOpen] = useState(false);
  const sprint = useQuery({ queryKey: queryKeys.sprint(sprintId), queryFn: () => sprintsApi.get(sprintId), enabled: Number.isFinite(sprintId) });
  const progress = useQuery({ queryKey: queryKeys.sprintProgress(sprintId), queryFn: () => sprintsApi.progress(sprintId), enabled: Number.isFinite(sprintId) });
  const tasks = useQuery({ queryKey: queryKeys.tasks(sprintId), queryFn: () => tasksApi.bySprint(sprintId), enabled: Number.isFinite(sprintId) });
  const form = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema), values: { name: sprint.data?.name ?? "", goal: sprint.data?.goal ?? "", endDate: sprint.data?.endDate?.slice(0, 10) ?? "" } });
  const invalidate = async () => { await queryClient.invalidateQueries({ queryKey: queryKeys.sprint(sprintId) }); await queryClient.invalidateQueries({ queryKey: queryKeys.sprintProgress(sprintId) }); };
  const update = useMutation({ mutationFn: (values: z.infer<typeof schema>) => sprintsApi.update(sprintId, values), onSuccess: async () => { await invalidate(); setEditOpen(false); toast.success("Sprint updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const start = useMutation({ mutationFn: () => sprintsApi.start(sprintId), onSuccess: async () => { await invalidate(); toast.success("Sprint started."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const complete = useMutation({ mutationFn: () => sprintsApi.complete(sprintId), onSuccess: async () => { await invalidate(); toast.success("Sprint completed."); }, onError: (error) => toast.error(getErrorMessage(error)) });

  if (sprint.isLoading) return <Skeleton className="h-96" />;
  if (sprint.isError || !sprint.data) return <ErrorState onRetry={() => void sprint.refetch()} />;

  const chartData = [
    { name: "Done", value: progress.data?.completedTasks ?? 0 },
    { name: "Remaining", value: Math.max((progress.data?.totalTasks ?? 0) - (progress.data?.completedTasks ?? 0), 0) },
  ];

  return (
    <>
      <PageHeader title={sprint.data.name} description={sprint.data.goal} actions={<><Button variant="outline" onClick={() => setEditOpen(true)}><Pencil className="h-4 w-4" />Edit</Button><Button variant="secondary" onClick={() => start.mutate()} disabled={start.isPending}><PlayCircle className="h-4 w-4" />Start</Button><Button onClick={() => complete.mutate()} disabled={complete.isPending}><CheckCircle2 className="h-4 w-4" />Complete</Button><Button asChild variant="outline"><Link to={`${routes.board(sprint.data.projectId)}?sprintId=${sprint.data.id}`}>Board</Link></Button></>} />
      <div className="grid gap-4 lg:grid-cols-[1fr_1fr]">
        <Card><CardHeader><CardTitle>Progress</CardTitle><CardDescription>{progress.data?.progressPercentage ?? 0}% complete</CardDescription></CardHeader><CardContent><div className="h-64"><ResponsiveContainer><PieChart><Pie data={chartData} dataKey="value" nameKey="name" innerRadius={55} outerRadius={90}><Cell fill="#0f766e" /><Cell fill="#f59e0b" /></Pie><Tooltip /></PieChart></ResponsiveContainer></div></CardContent></Card>
        <Card><CardHeader><CardTitle>Details</CardTitle></CardHeader><CardContent className="grid gap-3 text-sm"><p><Badge value={sprint.data.status} /></p><p>{formatDate(sprint.data.startDate)} - {formatDate(sprint.data.endDate)}</p><p>{tasks.data?.length ?? sprint.data.taskCount} tasks loaded from backend</p></CardContent></Card>
      </div>
      <Modal open={editOpen} onOpenChange={setEditOpen} title="Edit sprint"><form className="grid gap-4" onSubmit={form.handleSubmit((values) => update.mutate(values))}><Field label="Name"><Input {...form.register("name")} /></Field><Field label="Goal"><Textarea {...form.register("goal")} /></Field><Field label="End date"><Input type="date" {...form.register("endDate")} /></Field><Button disabled={update.isPending}>{update.isPending ? "Saving..." : "Save"}</Button></form></Modal>
    </>
  );
}
