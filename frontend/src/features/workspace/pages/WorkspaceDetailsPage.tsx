import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, UserMinus } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";
import { PageHeader } from "@/components/shared/PageHeader";
import { SearchInput } from "@/components/shared/SearchInput";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmDialog, Modal } from "@/components/ui/dialog";
import { Field, Input, SelectInput, Textarea } from "@/components/ui/forms";
import { Table, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { EmptyState, ErrorState, Skeleton } from "@/components/ui/state";
import { workspaceApi } from "@/features/workspace/api/workspaceApi";
import { formatDate } from "@/lib/utils";
import { getErrorMessage } from "@/services/apiClient";
import { UserRole } from "@/types/api";
import { queryKeys } from "@/utils/queryKeys";

const workspaceSchema = z.object({ name: z.string().min(1).max(100), description: z.string().max(500).optional() });
const memberSchema = z.object({ email: z.string().email(), role: z.number() });
const profileSchema = z.object({ firstName: z.string().max(50).optional(), lastName: z.string().max(50).optional(), phoneNumber: z.string().optional(), githubUsername: z.string().optional() });

export function WorkspaceDetailsPage() {
  const workspaceId = Number(useParams().workspaceId);
  const queryClient = useQueryClient();
  const [memberSearch, setMemberSearch] = useState("");
  const [editOpen, setEditOpen] = useState(false);
  const [memberOpen, setMemberOpen] = useState(false);
  const [profileUserId, setProfileUserId] = useState<string | null>(null);
  const [removeMemberEmail, setRemoveMemberEmail] = useState<string | null>(null);
  const workspace = useQuery({ queryKey: queryKeys.workspace(workspaceId), queryFn: () => workspaceApi.get(workspaceId), enabled: Number.isFinite(workspaceId) });
  const workspaceForm = useForm<z.infer<typeof workspaceSchema>>({ resolver: zodResolver(workspaceSchema), values: { name: workspace.data?.name ?? "", description: workspace.data?.description ?? "" } });
  const memberForm = useForm<z.infer<typeof memberSchema>>({ resolver: zodResolver(memberSchema), defaultValues: { email: "", role: UserRole.Developer } });
  const profileForm = useForm<z.infer<typeof profileSchema>>({ resolver: zodResolver(profileSchema), defaultValues: {} });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: queryKeys.workspace(workspaceId) });
  const updateWorkspace = useMutation({ mutationFn: (values: z.infer<typeof workspaceSchema>) => workspaceApi.update(workspaceId, values), onSuccess: async () => { await invalidate(); setEditOpen(false); toast.success("Workspace updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const addMember = useMutation({ mutationFn: (values: z.infer<typeof memberSchema>) => workspaceApi.addMember(workspaceId, { email: values.email, role: values.role as 0 | 1 | 2 }), onSuccess: async () => { await invalidate(); setMemberOpen(false); memberForm.reset(); toast.success("Member added."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const updateRole = useMutation({ mutationFn: ({ userId, role }: { userId: string; role: 0 | 1 | 2 }) => workspaceApi.updateMemberRole(workspaceId, userId, { role }), onSuccess: async () => { await invalidate(); toast.success("Role updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const removeMember = useMutation({ mutationFn: (email: string) => workspaceApi.removeMember(workspaceId, email), onSuccess: async () => { await invalidate(); setRemoveMemberEmail(null); toast.success("Member removed."); }, onError: (error) => toast.error(getErrorMessage(error)) });
  const updateProfile = useMutation({ mutationFn: (values: z.infer<typeof profileSchema>) => workspaceApi.updateMemberProfile(workspaceId, profileUserId ?? "", values), onSuccess: async () => { await invalidate(); setProfileUserId(null); toast.success("Member profile updated."); }, onError: (error) => toast.error(getErrorMessage(error)) });

  const members = useMemo(() => (workspace.data?.members ?? []).filter((member) => `${member.fullName} ${member.email} ${member.role}`.toLowerCase().includes(memberSearch.toLowerCase())), [memberSearch, workspace.data?.members]);

  if (workspace.isLoading) return <Skeleton className="h-96" />;
  if (workspace.isError || !workspace.data) return <ErrorState onRetry={() => void workspace.refetch()} />;

  return (
    <>
      <PageHeader title={workspace.data.name} description={workspace.data.description || "Workspace details"} actions={<><Button variant="outline" onClick={() => setEditOpen(true)}><Pencil className="h-4 w-4" />Edit</Button><Button onClick={() => setMemberOpen(true)}><Plus className="h-4 w-4" />Add member</Button><Button asChild variant="secondary"><Link to={`/workspaces/${workspaceId}/projects`}>Projects</Link></Button></>} />
      <div className="grid gap-4 lg:grid-cols-[1fr_2fr]">
        <Card><CardHeader><CardTitle>Summary</CardTitle></CardHeader><CardContent className="grid gap-2 text-sm"><p>Created {formatDate(workspace.data.createdAt)}</p><p>{workspace.data.projects.length} projects</p><p>{workspace.data.members.length} members</p></CardContent></Card>
        <Card>
          <CardHeader><CardTitle>Members</CardTitle></CardHeader>
          <CardContent>
            <div className="mb-4 max-w-sm"><SearchInput value={memberSearch} onChange={setMemberSearch} placeholder="Search members" /></div>
            {members.length === 0 ? <EmptyState title="No members found" /> : (
              <div className="overflow-x-auto">
                <Table><TableHeader><TableRow><TableHead>Name</TableHead><TableHead>Email</TableHead><TableHead>Role</TableHead><TableHead /></TableRow></TableHeader><tbody>
                  {members.map((member) => (
                    <TableRow key={member.userId}>
                      <TableCell>{member.fullName}</TableCell><TableCell>{member.email}</TableCell>
                      <TableCell><SelectInput value={String(roleValue(member.role))} onChange={(event) => updateRole.mutate({ userId: member.userId, role: Number(event.target.value) as 0 | 1 | 2 })}><option value={UserRole.Developer}>Developer</option><option value={UserRole.TeamLead}>Team Lead</option><option value={UserRole.Admin}>Admin</option></SelectInput></TableCell>
                      <TableCell className="text-right"><Button size="icon" variant="ghost" onClick={() => { setProfileUserId(member.userId); profileForm.reset({ firstName: member.fullName.split(" ")[0] ?? "", lastName: member.fullName.split(" ").slice(1).join(" "), phoneNumber: "", githubUsername: "" }); }}><Pencil className="h-4 w-4" /></Button><Button size="icon" variant="ghost" onClick={() => setRemoveMemberEmail(member.email)}><UserMinus className="h-4 w-4" /></Button></TableCell>
                    </TableRow>
                  ))}
                </tbody></Table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
      <Modal open={editOpen} onOpenChange={setEditOpen} title="Edit workspace"><form className="grid gap-4" onSubmit={workspaceForm.handleSubmit((values) => updateWorkspace.mutate(values))}><Field label="Name" error={workspaceForm.formState.errors.name?.message}><Input {...workspaceForm.register("name")} /></Field><Field label="Description" error={workspaceForm.formState.errors.description?.message}><Textarea {...workspaceForm.register("description")} /></Field><Button disabled={updateWorkspace.isPending}>{updateWorkspace.isPending ? "Saving..." : "Save"}</Button></form></Modal>
      <Modal open={memberOpen} onOpenChange={setMemberOpen} title="Add member" description="Enter the registered user's email address."><form className="grid gap-4" onSubmit={memberForm.handleSubmit((values) => addMember.mutate(values))}><Field label="Email" error={memberForm.formState.errors.email?.message}><Input type="email" autoComplete="email" {...memberForm.register("email")} /></Field><Field label="Role"><SelectInput {...memberForm.register("role", { valueAsNumber: true })}><option value={UserRole.Developer}>Developer</option><option value={UserRole.TeamLead}>Team Lead</option><option value={UserRole.Admin}>Admin</option></SelectInput></Field><Button disabled={addMember.isPending}>{addMember.isPending ? "Adding..." : "Add member"}</Button></form></Modal>
      <Modal open={profileUserId !== null} onOpenChange={(open) => !open && setProfileUserId(null)} title="Edit member profile"><form className="grid gap-4" onSubmit={profileForm.handleSubmit((values) => updateProfile.mutate(values))}><div className="grid gap-4 sm:grid-cols-2"><Field label="First name"><Input {...profileForm.register("firstName")} /></Field><Field label="Last name"><Input {...profileForm.register("lastName")} /></Field></div><Field label="Phone"><Input {...profileForm.register("phoneNumber")} /></Field><Field label="GitHub username"><Input {...profileForm.register("githubUsername")} /></Field><Button disabled={updateProfile.isPending}>{updateProfile.isPending ? "Saving..." : "Save profile"}</Button></form></Modal>
      <ConfirmDialog open={removeMemberEmail !== null} onOpenChange={(open) => !open && setRemoveMemberEmail(null)} title="Remove member" description={`This removes ${removeMemberEmail ?? "the selected member"} from the workspace if authorized.`} busy={removeMember.isPending} onConfirm={() => removeMemberEmail && removeMember.mutate(removeMemberEmail)} />
    </>
  );
}

function roleValue(role: string): 0 | 1 | 2 {
  if (role === "Admin") return UserRole.Admin;
  if (role === "TeamLead") return UserRole.TeamLead;
  return UserRole.Developer;
}
