import { DndContext, PointerSensor, closestCorners, useSensor, useSensors, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, arrayMove, horizontalListSortingStrategy, useSortable, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { GripVertical, Plus, Trash2 } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useParams, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmDialog, Modal } from "@/components/ui/dialog";
import { Field, Input, SelectInput, Textarea } from "@/components/ui/forms";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { boardApi } from "@/features/board/api/boardApi";
import { projectsApi } from "@/features/projects/api/projectsApi";
import { tasksApi } from "@/features/tasks/api/tasksApi";
import { TaskDetailModal } from "@/features/tasks/components/TaskDetailModal";
import { workspaceApi } from "@/features/workspace/api/workspaceApi";
import { cn, formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { ColumnResponse, ProjectTaskPriority, ProjectTaskStatus, TaskSummaryResponse } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

const columnSchema = z.object({ columnName: z.string().min(1).max(100) });
const renameSchema = z.object({ newName: z.string().min(1).max(100) });
const taskSchema = z.object({ title: z.string().min(1).max(200), description: z.string().max(2000).optional(), status: z.number(), priority: z.number(), dueDate: z.string().min(1), columnId: z.number() });

export function BoardPage() {
  const projectId = Number(useParams().projectId);
  const [params] = useSearchParams();
  const sprintId = Number(params.get("sprintId"));
  const queryClient = useQueryClient();
  const [taskId, setTaskId] = useState<number | null>(null);
  const [renameColumn, setRenameColumn] = useState<ColumnResponse | null>(null);
  const [deleteColumn, setDeleteColumn] = useState<number | null>(null);
  const [columnOpen, setColumnOpen] = useState(false);
  const [taskOpen, setTaskOpen] = useState(false);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }));
  const board = useQuery({ queryKey: queryKeys.board(projectId, sprintId), queryFn: () => boardApi.get(projectId, sprintId), enabled: Number.isFinite(projectId) && Number.isFinite(sprintId) && sprintId > 0 });
  const project = useQuery({ queryKey: queryKeys.project(projectId), queryFn: () => projectsApi.get(projectId), enabled: Number.isFinite(projectId) });
  const workspace = useQuery({ queryKey: project.data ? queryKeys.workspace(project.data.workspaceId) : ["workspace", "none"], queryFn: () => workspaceApi.get(project.data?.workspaceId ?? 0), enabled: Boolean(project.data?.workspaceId) });
  const columnForm = useForm<z.infer<typeof columnSchema>>({ resolver: zodResolver(columnSchema), defaultValues: { columnName: "" } });
  const renameForm = useForm<z.infer<typeof renameSchema>>({ resolver: zodResolver(renameSchema), values: { newName: renameColumn?.name ?? "" } });
  const taskForm = useForm<z.infer<typeof taskSchema>>({ resolver: zodResolver(taskSchema), values: { title: "", description: "", status: ProjectTaskStatus.Todo, priority: ProjectTaskPriority.Medium, dueDate: "", columnId: board.data?.columns[0]?.id ?? 0 } });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: queryKeys.board(projectId, sprintId) });
  const addColumn = useMutation({ mutationFn: (values: z.infer<typeof columnSchema>) => boardApi.addColumn(projectId, values), onSuccess: async () => { await invalidate(); setColumnOpen(false); columnForm.reset(); toast.success("Column created."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const updateColumn = useMutation({ mutationFn: (values: z.infer<typeof renameSchema>) => boardApi.updateColumn(renameColumn?.id ?? 0, values), onSuccess: async () => { await invalidate(); setRenameColumn(null); toast.success("Column renamed."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const removeColumn = useMutation({ mutationFn: boardApi.deleteColumn, onSuccess: async () => { await invalidate(); setDeleteColumn(null); toast.success("Column deleted."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const updateOrder = useMutation({ mutationFn: (orderedColumnIds: number[]) => boardApi.updateOrder(projectId, { orderedColumnIds }), onError: (error) => toast.error(getErrorMessage(error)) });
  const moveTask = useMutation({ mutationFn: ({ id, columnId }: { id: number; columnId: number }) => tasksApi.move(id, { columnId }), onSuccess: async () => { await invalidate(); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const createTask = useMutation({ mutationFn: (values: z.infer<typeof taskSchema>) => tasksApi.create(sprintId, { ...values, status: values.status as 0 | 1 | 2 | 3, priority: values.priority as 0 | 1 | 2 | 3, assigneeUserIds: [] }), onSuccess: async () => { await invalidate(); setTaskOpen(false); taskForm.reset(); toast.success("Task created."); }, onError: (error) => toast.error(getErrorMessage(error)) });

  const columns = useMemo(() => [...(board.data?.columns ?? [])].sort((a, b) => a.position - b.position), [board.data?.columns]);

  function onDragEnd(event: DragEndEvent) {
    const activeId = String(event.active.id);
    const overId = event.over ? String(event.over.id) : null;
    if (!overId || activeId === overId) return;
    if (activeId.startsWith("column:") && overId.startsWith("column:")) {
      const activeColumnId = Number(activeId.replace("column:", ""));
      const overColumnId = Number(overId.replace("column:", ""));
      const oldIndex = columns.findIndex((column) => column.id === activeColumnId);
      const newIndex = columns.findIndex((column) => column.id === overColumnId);
      const ordered = arrayMove(columns, oldIndex, newIndex).map((column) => column.id);
      updateOrder.mutate(ordered, { onSuccess: async () => invalidate() });
      return;
    }
    if (activeId.startsWith("task:")) {
      const task = columns.flatMap((column) => column.tasks).find((item) => `task:${item.id}` === activeId);
      const overTask = columns.flatMap((column) => column.tasks).find((item) => `task:${item.id}` === overId);
      const overColumn = columns.find((column) => `column:${column.id}` === overId || column.id === overTask?.columnId);
      if (task && overColumn && task.columnId !== overColumn.id) moveTask.mutate({ id: task.id, columnId: overColumn.id });
    }
  }

  if (!sprintId) return <EmptyState title="Select a sprint to view the board" description="Open a project sprint and use its Board action so the backend can return sprint-scoped columns and tasks." />;
  if (board.isLoading) return <Skeleton className="h-[70vh]" />;
  if (board.isError) return <ErrorState onRetry={() => void board.refetch()} />;

  return (
    <>
      <PageHeader title="Kanban board" description="Drag tasks between columns and reorder columns. All changes persist through the backend." actions={<><Button variant="outline" onClick={() => setColumnOpen(true)}><Plus className="h-4 w-4" />Column</Button><Button onClick={() => setTaskOpen(true)}><Plus className="h-4 w-4" />Task</Button></>} />
      <DndContext sensors={sensors} collisionDetection={closestCorners} onDragEnd={onDragEnd}>
        <SortableContext items={columns.map((column) => `column:${column.id}`)} strategy={horizontalListSortingStrategy}>
          <div className="flex gap-4 overflow-x-auto pb-4">
            {columns.map((column) => <SortableColumn key={column.id} column={column} onRename={() => setRenameColumn(column)} onDelete={() => setDeleteColumn(column.id)} onTaskOpen={setTaskId} />)}
          </div>
        </SortableContext>
      </DndContext>
      <Modal open={columnOpen} onOpenChange={setColumnOpen} title="Create column"><form className="grid gap-4" onSubmit={columnForm.handleSubmit((values) => addColumn.mutate(values))}><Field label="Column name"><Input {...columnForm.register("columnName")} /></Field><Button disabled={addColumn.isPending}>{addColumn.isPending ? "Creating..." : "Create"}</Button></form></Modal>
      <Modal open={renameColumn !== null} onOpenChange={(open) => !open && setRenameColumn(null)} title="Rename column"><form className="grid gap-4" onSubmit={renameForm.handleSubmit((values) => updateColumn.mutate(values))}><Field label="New name"><Input {...renameForm.register("newName")} /></Field><Button disabled={updateColumn.isPending}>{updateColumn.isPending ? "Saving..." : "Save"}</Button></form></Modal>
      <Modal open={taskOpen} onOpenChange={setTaskOpen} title="Create task"><form className="grid gap-4" onSubmit={taskForm.handleSubmit((values) => createTask.mutate(values))}><Field label="Title"><Input {...taskForm.register("title")} /></Field><Field label="Description"><Textarea {...taskForm.register("description")} /></Field><div className="grid gap-4 sm:grid-cols-2"><Field label="Column"><SelectInput {...taskForm.register("columnId", { valueAsNumber: true })}>{columns.map((column) => <option key={column.id} value={column.id}>{column.name}</option>)}</SelectInput></Field><Field label="Due date"><Input type="date" {...taskForm.register("dueDate")} /></Field></div><div className="grid gap-4 sm:grid-cols-2"><Field label="Status"><SelectInput {...taskForm.register("status", { valueAsNumber: true })}><option value={ProjectTaskStatus.Todo}>Todo</option><option value={ProjectTaskStatus.InProgress}>In progress</option><option value={ProjectTaskStatus.Done}>Done</option><option value={ProjectTaskStatus.Cancelled}>Cancelled</option></SelectInput></Field><Field label="Priority"><SelectInput {...taskForm.register("priority", { valueAsNumber: true })}><option value={ProjectTaskPriority.Low}>Low</option><option value={ProjectTaskPriority.Medium}>Medium</option><option value={ProjectTaskPriority.High}>High</option><option value={ProjectTaskPriority.Critical}>Critical</option></SelectInput></Field></div><Button disabled={createTask.isPending}>{createTask.isPending ? "Creating..." : "Create"}</Button></form></Modal>
      <ConfirmDialog open={deleteColumn !== null} onOpenChange={(open) => !open && setDeleteColumn(null)} title="Delete column" description="This deletes the board column if authorized by the backend." busy={removeColumn.isPending} onConfirm={() => deleteColumn && removeColumn.mutate(deleteColumn)} />
      <TaskDetailModal taskId={taskId} open={taskId !== null} onOpenChange={(open) => !open && setTaskId(null)} onChanged={() => void invalidate()} workspaceMembers={workspace.data?.members ?? []} />
    </>
  );
}

function SortableColumn({ column, onRename, onDelete, onTaskOpen }: { column: ColumnResponse; onRename: () => void; onDelete: () => void; onTaskOpen: (id: number) => void }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: `column:${column.id}` });
  return (
    <Card ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} className={cn("w-80 shrink-0", isDragging && "opacity-70")}>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <div className="flex items-center gap-2"><button {...attributes} {...listeners} aria-label="Drag column"><GripVertical className="h-4 w-4 text-muted-foreground" /></button><CardTitle>{column.name}</CardTitle><Badge value={`${column.tasks.length}`} /></div>
        <div className="flex gap-1"><Button size="sm" variant="ghost" onClick={onRename}>Rename</Button><Button size="icon" variant="ghost" onClick={onDelete}><Trash2 className="h-4 w-4" /></Button></div>
      </CardHeader>
      <CardContent>
        <SortableContext items={column.tasks.map((task) => `task:${task.id}`)} strategy={verticalListSortingStrategy}>
          <div className="grid min-h-24 gap-3">
            {column.tasks.map((task) => <TaskCard key={task.id} task={task} onOpen={() => onTaskOpen(task.id)} />)}
          </div>
        </SortableContext>
      </CardContent>
    </Card>
  );
}

function TaskCard({ task, onOpen }: { task: TaskSummaryResponse; onOpen: () => void }) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: `task:${task.id}` });
  return (
    <button ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} {...attributes} {...listeners} onClick={onOpen} className={cn("rounded-md border bg-background p-3 text-left shadow-sm transition hover:border-primary", isDragging && "opacity-70")}>
      <p className="font-medium">{task.title}</p>
      <div className="mt-3 flex flex-wrap gap-2"><Badge value={task.priority} /><Badge value={task.status} /><span className="text-xs text-muted-foreground">{formatDate(task.dueDate)}</span></div>
    </button>
  );
}
