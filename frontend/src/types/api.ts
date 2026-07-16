export type ProjectStatusName = "InProgress" | "Completed" | "OnHold" | "Cancelled";
export type SprintStatusName = "Planning" | "Active" | "Completed" | "Cancelled";
export type ProjectTaskStatusName = "Todo" | "InProgress" | "Done" | "Cancelled";
export type ProjectTaskPriorityName = "Low" | "Medium" | "High" | "Critical";
export type ProjectTaskApprovalStatusName = "Pending" | "Approved" | "Rejected";
export type UserRoleName = "Developer" | "TeamLead" | "Admin";

export const ProjectStatus = { InProgress: 0, Completed: 1, OnHold: 2, Cancelled: 3 } as const;
export const SprintStatus = { Planning: 0, Active: 1, Completed: 2, Cancelled: 3 } as const;
export const ProjectTaskStatus = { Todo: 0, InProgress: 1, Done: 2, Cancelled: 3 } as const;
export const ProjectTaskPriority = { Low: 0, Medium: 1, High: 2, Critical: 3 } as const;
export const ProjectTaskApprovalStatus = { Pending: 0, Approved: 1, Rejected: 2 } as const;
export const UserRole = { Developer: 0, TeamLead: 1, Admin: 2 } as const;

export type ProjectStatusValue = (typeof ProjectStatus)[keyof typeof ProjectStatus];
export type SprintStatusValue = (typeof SprintStatus)[keyof typeof SprintStatus];
export type ProjectTaskStatusValue = (typeof ProjectTaskStatus)[keyof typeof ProjectTaskStatus];
export type ProjectTaskPriorityValue = (typeof ProjectTaskPriority)[keyof typeof ProjectTaskPriority];
export type ProjectTaskApprovalStatusValue = (typeof ProjectTaskApprovalStatus)[keyof typeof ProjectTaskApprovalStatus];
export type UserRoleValue = (typeof UserRole)[keyof typeof UserRole];

export interface RegisterRequestDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface RefreshRequestDto {
  accessToken: string;
  refreshToken: string;
}

export interface LogoutRequestDto {
  refreshToken: string;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  role: string;
}

export interface RegisterResponseDto {
  userId: string;
  email: string;
  requiresEmailConfirmation: true;
  message: string;
}

export interface ConfirmEmailResponseDto {
  email: string;
  confirmed: boolean;
  message: string;
}

export interface ResendEmailConfirmationRequestDto {
  email: string;
}

export interface OAuthLoginRequestDto {
  code: string;
  redirectUri: string;
}

export interface AccountResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string | null;
  profilePicture?: string | null;
  dob?: string | null;
  githubUsername?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpdateAccountRequest {
  firstName?: string | null;
  lastName?: string | null;
  phoneNumber?: string | null;
  profilePicture?: string | null;
  dob?: string | null;
  githubUsername?: string | null;
}

export interface CreateWorkspaceRequest {
  name: string;
  description?: string | null;
}

export type UpdateWorkspaceRequest = CreateWorkspaceRequest;

export interface AddWorkspaceMemberRequest {
  email?: string | null;
  userId?: string | null;
  role: UserRoleValue;
}

export interface WorkspaceMemberResponse {
  userId: string;
  fullName: string;
  email: string;
  profilePicture?: string | null;
  role: string;
  joinedAt: string;
}

export interface WorkspaceProjectResponse {
  id: number;
  name: string;
  description: string;
  status: string;
  startDate: string;
  endDate: string;
}

export interface WorkspaceResponse {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  projects: WorkspaceProjectResponse[];
  members: WorkspaceMemberResponse[];
}

export interface WorkspaceSummaryResponse {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  projectCount: number;
  memberCount: number;
  currentUserRole: string;
}

export interface UpdateWorkspaceMemberRoleRequest {
  role: UserRoleValue;
}

export interface WorkspaceMemberDetailResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string | null;
  profilePicture?: string | null;
  dob?: string | null;
  githubUsername?: string | null;
  role: string;
}

export interface CreateProjectRequest {
  name: string;
  description?: string | null;
  status: ProjectStatusValue;
  startDate: string;
  endDate: string;
  workspaceId: number;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string | null;
  status: ProjectStatusValue;
  endDate: string;
}

export interface ProjectResponse {
  id: number;
  name: string;
  description: string;
  status: string;
  startDate: string;
  endDate: string;
  workspaceId: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateSprintRequest {
  name: string;
  goal: string;
  startDate: string;
  endDate: string;
}

export interface UpdateSprintRequest {
  name: string;
  goal: string;
  endDate: string;
}

export interface SprintResponse {
  id: number;
  name: string;
  goal: string;
  status: string;
  startDate: string;
  endDate: string;
  projectId: number;
  taskCount: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SprintProgressResponse {
  sprintId: number;
  totalTasks: number;
  completedTasks: number;
  progressPercentage: number;
}

export interface AddColumnRequest {
  columnName: string;
}

export interface UpdateColumnRequest {
  newName: string;
}

export interface UpdateColumnOrderRequest {
  orderedColumnIds: number[];
}

export interface TaskAssigneeResponse {
  userId: string;
  email?: string | null;
  fullName: string;
}

export interface TaskSummaryResponse {
  id: number;
  title: string;
  status: string;
  priority: string;
  dueDate: string;
  sprintId: number;
  columnId: number;
  approvalStatus?: ProjectTaskApprovalStatusName | null;
  assignees: TaskAssigneeResponse[];
  visibilityReasons: string[];
}

export interface ColumnResponse {
  id: number;
  name: string;
  position: number;
  tasks: TaskSummaryResponse[];
}

export interface GetBoardDetailsResponse {
  columns: ColumnResponse[];
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  status: ProjectTaskStatusValue;
  priority: ProjectTaskPriorityValue;
  dueDate: string;
  columnId: number;
  assigneeUserIds: string[];
}

export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  status: ProjectTaskStatusValue;
  priority: ProjectTaskPriorityValue;
  dueDate: string;
}

export interface UpdateTaskStatusRequest {
  status: ProjectTaskStatusValue;
}

export interface SubmitTaskRequest {
  commitHash: string;
}

export interface ReviewTaskRequest {
  approvalStatus: ProjectTaskApprovalStatusValue;
  comment: string;
}

export interface MoveTaskRequest {
  columnId: number;
}

export interface AssignTaskRequest {
  userId: string;
}

export interface TaskDependencyResponse {
  dependencyTaskId: number;
  title: string;
  status: string;
  approvalStatus?: ProjectTaskApprovalStatusName | null;
}

export interface TaskDetailResponse extends TaskSummaryResponse {
  description: string;
  createdAt: string;
  updatedAt?: string | null;
  dependencies: TaskDependencyResponse[];
  commits: TaskCommitResponse[];
  comments: TaskCommentResponse[];
}

export interface TaskActivityLogResponse {
  id: number;
  fieldChanged: string;
  oldValue: string;
  newValue: string;
  appUserId: string;
  appUserName: string;
  createdAt: string;
}

export interface TaskCommitResponse {
  id: number;
  commitHash: string;
  status: string;
  appUserId: string;
  appUserName: string;
  createdAt: string;
}

export interface TaskCommentResponse {
  id: number;
  content: string;
  appUserId: string;
  appUserName: string;
  createdAt: string;
}

export interface DashboardSummaryResponse {
  workspaces: WorkspaceSummaryResponse[];
  projects: ProjectResponse[];
  sprints: SprintResponse[];
  tasks: TaskSummaryResponse[];
  assignedTasks: TaskSummaryResponse[];
}

export interface MyTaskResponse extends TaskSummaryResponse {
  workspaceId: number;
  workspaceName: string;
  projectName: string;
  sprintName: string;
  workspaceMembers: WorkspaceMemberResponse[];
}
