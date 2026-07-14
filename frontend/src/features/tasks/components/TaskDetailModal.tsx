import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link2, UserPlus, X } from "lucide-react";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Modal } from "@/components/ui/dialog";
import { Field, Input, SelectInput, Textarea } from "@/components/ui/forms";
import { Skeleton } from "@/components/ui/state";
import { tasksApi } from "@/features/tasks/api/tasksApi";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { ProjectTaskPriority, ProjectTaskStatus, type WorkspaceMemberResponse } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

const taskSchema = z.object({ title: z.string().min(1).max(200), description: z.string().max(2000).optional(), status: z.number(), priority: z.number(), dueDate: z.string().min(1) });
const userSchema = z.object({ userId: z.string().min(1) });
const dependencySchema = z.object({ dependencyTaskId: z.number().positive() });

export function TaskDetailModal({
  taskId,
  open,
  onOpenChange,
  onChanged,
  workspaceMembers = [],
}: {
  taskId: number | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onChanged?: () => void;
  workspaceMembers?: WorkspaceMemberResponse[];
}) {
  const queryClient = useQueryClient();
  const enabled = open && taskId !== null;
  const task = useQuery({ queryKey: taskId ? queryKeys.task(taskId) : ["task", "none"], queryFn: () => tasksApi.get(taskId ?? 0), enabled });
  const activity = useQuery({ queryKey: taskId ? queryKeys.taskActivity(taskId) : ["task-activity", "none"], queryFn: () => tasksApi.activity(taskId ?? 0), enabled });
  const form = useForm<z.infer<typeof taskSchema>>({ resolver: zodResolver(taskSchema), defaultValues: { title: "", description: "", status: ProjectTaskStatus.Todo, priority: ProjectTaskPriority.Medium, dueDate: "" } });
  const userForm = useForm<z.infer<typeof userSchema>>({ resolver: zodResolver(userSchema), defaultValues: { userId: "" } });
  const dependencyForm = useForm<z.infer<typeof dependencySchema>>({ resolver: zodResolver(dependencySchema), defaultValues: { dependencyTaskId: 0 } });

  useEffect(() => {
    if (task.data) {
      form.reset({ title: task.data.title, description: task.data.description ?? "", status: statusValue(task.data.status), priority: priorityValue(task.data.priority), dueDate: task.data.dueDate.slice(0, 10) });
    }
  }, [form, task.data]);

  const invalidate = async () => {
    if (!taskId) return;
    await queryClient.invalidateQueries({ queryKey: queryKeys.task(taskId) });
    await queryClient.invalidateQueries({ queryKey: queryKeys.taskActivity(taskId) });
    onChanged?.();
  };
  const update = useMutation({ mutationFn: (values: z.infer<typeof taskSchema>) => tasksApi.update(taskId ?? 0, { ...values, status: values.status as 0 | 1 | 2 | 3, priority: values.priority as 0 | 1 | 2 | 3 }), onSuccess: async () => { await invalidate(); toast.success("Task updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const assign = useMutation({ mutationFn: (values: z.infer<typeof userSchema>) => tasksApi.assign(taskId ?? 0, values), onSuccess: async () => { await invalidate(); userForm.reset(); toast.success("Assignee added."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const unassign = useMutation({ mutationFn: (userId: string) => tasksApi.unassign(taskId ?? 0, userId), onSuccess: async () => { await invalidate(); toast.success("Assignee removed."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const addDependency = useMutation({ mutationFn: (values: z.infer<typeof dependencySchema>) => tasksApi.addDependency(taskId ?? 0, values.dependencyTaskId), onSuccess: async () => { await invalidate(); dependencyForm.reset(); toast.success("Dependency added."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const removeDependency = useMutation({ mutationFn: (dependencyTaskId: number) => tasksApi.removeDependency(taskId ?? 0, dependencyTaskId), onSuccess: async () => { await invalidate(); toast.success("Dependency removed."); }, onError: (error) => toast.error(getErrorMessage(error)) });

  return (
    <Modal open={open} onOpenChange={onOpenChange} title={task.data?.title ?? "Task details"} className="max-h-[90vh] max-w-3xl overflow-y-auto">
      {task.isLoading ? <Skeleton className="h-96" /> : null}
      {task.data ? (
        <div className="grid gap-6">
          <form className="grid gap-4" onSubmit={form.handleSubmit((values) => update.mutate(values))}>
            <Field label="Title"><Input {...form.register("title")} /></Field>
            <Field label="Description"><Textarea {...form.register("description")} /></Field>
            <div className="grid gap-4 sm:grid-cols-3">
              <Field label="Status"><SelectInput {...form.register("status", { valueAsNumber: true })}><option value={ProjectTaskStatus.Todo}>Todo</option><option value={ProjectTaskStatus.InProgress}>In progress</option><option value={ProjectTaskStatus.Done}>Done</option><option value={ProjectTaskStatus.Cancelled}>Cancelled</option></SelectInput></Field>
              <Field label="Priority"><SelectInput {...form.register("priority", { valueAsNumber: true })}><option value={ProjectTaskPriority.Low}>Low</option><option value={ProjectTaskPriority.Medium}>Medium</option><option value={ProjectTaskPriority.High}>High</option><option value={ProjectTaskPriority.Critical}>Critical</option></SelectInput></Field>
              <Field label="Due date"><Input type="date" {...form.register("dueDate")} /></Field>
            </div>
            <Button className="w-fit" disabled={update.isPending}>{update.isPending ? "Saving..." : "Save task"}</Button>
          </form>
          <section className="grid gap-3">
            <h3 className="text-sm font-semibold">Assignees</h3>
            <div className="flex flex-wrap gap-2">{task.data.assignees.map((assignee) => <span key={assignee.userId} className="inline-flex items-center gap-2 rounded-full bg-muted px-3 py-1 text-sm">{assignee.fullName || assignee.email || assignee.userId}<button onClick={() => unassign.mutate(assignee.userId)} aria-label="Remove assignee"><X className="h-3 w-3" /></button></span>)}</div>
            <form className="flex gap-2" onSubmit={userForm.handleSubmit((values) => assign.mutate(values))}>
              <SelectInput className="min-w-0 flex-1" {...userForm.register("userId")}>
                <option value="">Select a workspace member</option>
                {workspaceMembers
                  .filter((member) => !task.data.assignees.some((assignee) => assignee.userId === member.userId))
                  .map((member) => (
                    <option key={member.userId} value={member.userId}>
                      {member.fullName || member.email} ({member.email})
                    </option>
                  ))}
              </SelectInput>
              <Button disabled={assign.isPending || workspaceMembers.length === 0}><UserPlus className="h-4 w-4" />Assign</Button>
            </form>
          </section>
          <section className="grid gap-3">
            <h3 className="text-sm font-semibold">Dependencies</h3>
            <div className="grid gap-2">{task.data.dependencies.map((dependency) => <div key={dependency.dependencyTaskId} className="flex items-center justify-between rounded-md border p-3"><span><Badge value={dependency.status} /> <span className="ml-2">{dependency.title}</span></span><Button size="icon" variant="ghost" onClick={() => removeDependency.mutate(dependency.dependencyTaskId)}><X className="h-4 w-4" /></Button></div>)}</div>
            <form className="flex gap-2" onSubmit={dependencyForm.handleSubmit((values) => addDependency.mutate(values))}><Input type="number" placeholder="Dependency task id" {...dependencyForm.register("dependencyTaskId", { valueAsNumber: true })} /><Button><Link2 className="h-4 w-4" />Add</Button></form>
          </section>
          <section className="grid gap-3">
            <h3 className="text-sm font-semibold">Activity timeline</h3>
            <div className="grid gap-2">{(activity.data ?? []).map((log) => <div key={log.id} className="rounded-md border p-3 text-sm"><p className="font-medium">{log.fieldChanged}: {log.oldValue || "empty"} {"->"} {log.newValue || "empty"}</p><p className="text-muted-foreground">{log.appUserName} at {formatDate(log.createdAt)}</p></div>)}</div>
          </section>
        </div>
      ) : null}
    </Modal>
  );
}

function statusValue(status: string): 0 | 1 | 2 | 3 {
  if (status === "InProgress") return ProjectTaskStatus.InProgress;
  if (status === "Done") return ProjectTaskStatus.Done;
  if (status === "Cancelled") return ProjectTaskStatus.Cancelled;
  return ProjectTaskStatus.Todo;
}

function priorityValue(priority: string): 0 | 1 | 2 | 3 {
  if (priority === "Low") return ProjectTaskPriority.Low;
  if (priority === "High") return ProjectTaskPriority.High;
  if (priority === "Critical") return ProjectTaskPriority.Critical;
  return ProjectTaskPriority.Medium;
}
