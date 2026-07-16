import type { AccountResponse, WorkspaceMemberResponse, WorkspaceResponse } from "@/types/api";

export function getCurrentWorkspaceMember(workspace?: WorkspaceResponse, account?: AccountResponse): WorkspaceMemberResponse | undefined {
  if (!workspace || !account) return undefined;
  return workspace.members.find((member) => member.userId === account.userId);
}

export function isWorkspaceAdmin(role?: string): boolean {
  return role === "Admin";
}

export function isWorkspaceManager(role?: string): boolean {
  return role === "Admin" || role === "TeamLead";
}
