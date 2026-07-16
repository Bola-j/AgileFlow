import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { PageHeader } from "@/components/shared/PageHeader";
import { SearchInput } from "@/components/shared/SearchInput";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { accountApi } from "@/features/account/api/accountApi";
import { TaskDetailModal } from "@/features/tasks/components/TaskDetailModal";
import { isWorkspaceManager } from "@/features/workspace/utils/permissions";
import { formatDate } from "@/lib/utils";
import { dashboardApi } from "@/services/dashboardApi";
import { queryKeys } from "@/utils/queryKeys";

export function TasksPage() {
  const [search, setSearch] = useState("");
  const [taskId, setTaskId] = useState<number | null>(null);
  const query = useQuery({ queryKey: queryKeys.myTasks, queryFn: dashboardApi.myTasks });
  const account = useQuery({ queryKey: queryKeys.account, queryFn: accountApi.me });
  const tasks = useMemo(() => (query.data ?? []).filter((task) => `${task.title} ${task.projectName} ${task.sprintName} ${task.status}`.toLowerCase().includes(search.toLowerCase())), [query.data, search]);
  const selectedTask = query.data?.find((task) => task.id === taskId);
  const currentMember = selectedTask?.workspaceMembers.find((member) => member.userId === account.data?.userId);
  const canManageSelectedTask = isWorkspaceManager(currentMember?.role);

  return (
    <>
      <PageHeader title="My Tasks" description="Tasks assigned to your account, loaded from real sprint task endpoints." />
      <div className="mb-5 max-w-md"><SearchInput value={search} onChange={setSearch} placeholder="Search assigned tasks" /></div>
      {query.isLoading ? <Skeleton className="h-96" /> : null}
      {query.isError ? <ErrorState onRetry={() => void query.refetch()} /> : null}
      {!query.isLoading && tasks.length === 0 ? <EmptyState title="No assigned tasks found" description="Assigned tasks appear here after project leads assign them through the task details flow." /> : null}
      <div className="grid gap-3">
        {tasks.map((task) => (
          <button key={task.id} className="text-left" onClick={() => setTaskId(task.id)}>
            <Card className="transition hover:border-primary">
              <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
                <div><p className="font-medium">{task.title}</p><p className="text-sm text-muted-foreground">{task.workspaceName} / {task.projectName} / {task.sprintName}</p></div>
                <div className="flex flex-wrap gap-2"><Badge value={task.status} /><Badge value={task.priority} /><Badge value={formatDate(task.dueDate)} /></div>
              </CardContent>
            </Card>
          </button>
        ))}
      </div>
      <TaskDetailModal taskId={taskId} open={taskId !== null} onOpenChange={(open) => !open && setTaskId(null)} onChanged={() => void query.refetch()} workspaceMembers={selectedTask?.workspaceMembers ?? []} availableTasks={(query.data ?? []).filter((task) => task.sprintId === selectedTask?.sprintId)} canManage={canManageSelectedTask} />
    </>
  );
}
