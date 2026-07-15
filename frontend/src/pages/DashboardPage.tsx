import { useQuery } from "@tanstack/react-query";
import { Bar, BarChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { PageHeader } from "@/components/shared/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { formatDate } from "@/lib/utils";
import { dashboardApi } from "@/services/dashboardApi";
import type { TaskSummaryResponse } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

export function DashboardPage() {
  const query = useQuery({ queryKey: queryKeys.dashboard, queryFn: dashboardApi.summary });
  if (query.isLoading) return <Skeleton className="h-[70vh]" />;
  if (query.isError || !query.data) return <ErrorState onRetry={() => void query.refetch()} />;
  const activeProjects = query.data.projects.filter((project) => project.status === "InProgress").length;
  const activeSprint = query.data.sprints.find((sprint) => sprint.status === "Active");
  const statusData = statusDistribution(query.data.tasks);
  const sprintData = query.data.sprints.slice(0, 8).map((sprint) => ({ name: sprint.name, tasks: sprint.taskCount }));

  return (
    <>
      <PageHeader title="Dashboard" description="Operational summary from the dashboard aggregate API." />
      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Metric title="Total projects" value={query.data.projects.length} />
        <Metric title="Active projects" value={activeProjects} />
        <Metric title="Active sprint" value={activeSprint?.name ?? "None"} />
        <Metric title="Assigned tasks" value={query.data.assignedTasks.length} />
      </div>
      <div className="grid gap-4 xl:grid-cols-2">
        <Card><CardHeader><CardTitle>Task Status Distribution</CardTitle></CardHeader><CardContent><div className="h-72">{statusData.length ? <ResponsiveContainer><PieChart><Pie data={statusData} dataKey="value" nameKey="name" innerRadius={55} outerRadius={95}>{statusData.map((entry, index) => <Cell key={entry.name} fill={["#0f766e", "#0284c7", "#f59e0b", "#dc2626"][index % 4]} />)}</Pie><Tooltip /></PieChart></ResponsiveContainer> : <EmptyState title="No task data" />}</div></CardContent></Card>
        <Card><CardHeader><CardTitle>Sprint Workload</CardTitle></CardHeader><CardContent><div className="h-72">{sprintData.length ? <ResponsiveContainer><BarChart data={sprintData}><CartesianGrid strokeDasharray="3 3" /><XAxis dataKey="name" tick={{ fontSize: 11 }} /><YAxis allowDecimals={false} /><Tooltip /><Bar dataKey="tasks" fill="#0f766e" radius={[4, 4, 0, 0]} /></BarChart></ResponsiveContainer> : <EmptyState title="No sprint data" />}</div></CardContent></Card>
      </div>
      <Card className="mt-4"><CardHeader><CardTitle>Recent backend-backed items</CardTitle></CardHeader><CardContent className="grid gap-3">{query.data.projects.slice(0, 5).map((project) => <div key={project.id} className="flex flex-col gap-2 rounded-md border p-3 sm:flex-row sm:items-center sm:justify-between"><div><p className="font-medium">{project.name}</p><p className="text-sm text-muted-foreground">Updated {formatDate(project.updatedAt ?? project.createdAt)}</p></div><Badge value={project.status} /></div>)}</CardContent></Card>
    </>
  );
}

function Metric({ title, value }: { title: string; value: number | string }) {
  return <Card><CardHeader><CardTitle className="text-sm text-muted-foreground">{title}</CardTitle></CardHeader><CardContent><p className="text-2xl font-semibold">{value}</p></CardContent></Card>;
}

function statusDistribution(tasks: TaskSummaryResponse[]) {
  return Object.entries(tasks.reduce<Record<string, number>>((result, task) => ({ ...result, [task.status]: (result[task.status] ?? 0) + 1 }), {})).map(([name, value]) => ({ name, value }));
}
