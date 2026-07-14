# AgileFlow Full Upcoming Development Plan

## Summary
The next work should be executed as a sequence of backend-first vertical slices, then frontend integration, then demo hardening. The codebase already has authentication, workspace/project/task/sprint foundations, so the remaining work is to complete execution workflows, collaboration, automation, GitHub integration, and presentation-ready notifications. The safest delivery order is:

1. stabilize the current backend baseline  
2. complete project execution features  
3. add automation and collaboration  
4. add GitHub + notifications + email  
5. build the frontend on settled API contracts  
6. harden for demo and local deployment

The MVP is done when a team can log in, create a workspace and project, create sprints, manage a board, create and move tasks, assign or auto-assign work, add dependencies and comments, receive in-app and local email notifications, link GitHub commits to tasks, approve/reject commits, and demo the full flow locally.

## Phase 1: Stabilize the Current Backend Baseline
- Normalize package versions before more feature growth:
  - resolve the current `AutoMapper` / `AutoMapper.Extensions.Microsoft.DependencyInjection` version mismatch
  - remove the remaining vulnerable `AutoMapper 12.0.1` references
- Clean up domain/file inconsistencies:
  - rename `ProjectTask .cs` to `ProjectTask.cs`
  - normalize namespaces under the same convention already used across the solution
- Keep AutoMapper as the standard mapping approach for all new slices.
- Fix development configuration drift:
  - confirm CORS origin for the eventual frontend dev server
  - add any missing config sections for GitHub webhook secret and local email SMTP
- Add focused backend tests around the slices already implemented:
  - workspace authorization
  - task authorization
  - sprint rules

**Acceptance**
- `dotnet build backend\AgileFlow.sln` passes without package mismatch warnings.
- Existing auth/workspace/project/task/sprint flows still work after cleanup.

## Phase 2: Project Completion Board
- Add `IBoardRepository`, `IBoardService`, `BoardController`.
- Implement:
  - `POST /api/projects/{projectId}/board`
  - `GET /api/projects/{projectId}/board`
  - `POST /api/boards/{boardId}/columns`
  - `PUT /api/columns/{id}`
  - `DELETE /api/columns/{id}`
- Board creation must seed:
  - `To Do`
  - `In Progress`
  - `Done`
- `GET /api/projects/{projectId}/board` must return a frontend-ready shape:
  - project metadata
  - board metadata
  - ordered columns
  - tasks grouped by column
  - task assignees and sprint linkage needed by the UI
- Enforce board permissions through the existing workspace authorization service:
  - members can read
  - `Admin` and `TeamLead` can create/update/delete board structure
- If only one board per project is intended, enforce that rule in the service layer.

**Acceptance**
- A project can create one usable board.
- Default columns are created automatically.
- Existing tasks appear in the expected columns.
- Board payload is ready to drive a Kanban UI directly.

## Phase 3: Task Dependencies and Activity
- Add dependency support:
  - `POST /api/tasks/{id}/dependencies`
  - `DELETE /api/tasks/{id}/dependencies/{dependencyTaskId}`
- Enforce dependency rules:
  - same project only
  - no self-dependency
  - no duplicate dependency
  - no cycle
- Add activity log read support:
  - `GET /api/tasks/{id}/activity`
- Record activity entries for:
  - title changes
  - description changes
  - status changes
  - priority changes
  - due date changes
  - column moves
  - assignee add/remove
  - dependency add/remove
- Keep task authorization consistent with the current model:
  - members can read
  - `Admin` and `TeamLead` manage structure
  - assigned developers can progress their tasks where already allowed

**Acceptance**
- Invalid dependencies are rejected with clear messages.
- Task activity shows a readable mutation history.
- Dependency-aware task state is usable by later auto-assign logic.

## Phase 4: Auto-Assign
- Add `POST /api/sprints/{id}/auto-assign`.
- Implement `IAutoAssignmentService` or equivalent dedicated service.
- Assignment rules:
  - caller must be `Admin` or `TeamLead`
  - only workspace members with role `Developer` are candidates
  - only tasks in the target sprint are considered
  - skip deleted tasks
  - skip already assigned tasks
  - skip tasks blocked by unfinished dependencies
  - choose the developer with the lowest active workload
- Make tie-breaking deterministic:
  - sort by workload ascending
  - then stable user identifier ordering
- Return a summary response containing:
  - assigned tasks
  - assigned users
  - skipped tasks with reason

**Acceptance**
- Auto-assign only works for managers.
- Blocked or already-assigned tasks remain unchanged.
- Assignment summary is explicit enough for UI display and debugging.

## Phase 5: Comments, Notifications, and Alerts
- Add comments support:
  - `GET /api/tasks/{id}/comments`
  - `POST /api/tasks/{id}/comments`
  - `PUT /api/comments/{id}`
  - `DELETE /api/comments/{id}`
- Comment permissions:
  - workspace members can read
  - workspace members can create
  - only author can edit
  - author, `Admin`, or `TeamLead` can delete
- Add notification service/controller if not already present:
  - `GET /api/notifications`
  - `PUT /api/notifications/{id}/read`
  - `PUT /api/notifications/read-all`
- Keep `Notification` as the primary persisted notification model.
- Trigger in-app notifications for:
  - manual task assignment
  - auto-assigned task
  - comment creation
  - commit linked to task
  - commit approved
  - commit rejected
- Keep notifications polling-based for the MVP.

**Acceptance**
- Users can collaborate on tasks through comments.
- Relevant events create unread notifications.
- Read and read-all only affect the current user.

## Phase 6: GitHub Integration and Local Email Notifications
- Add GitHub webhook ingestion:
  - `POST /api/webhooks/github`
- Handle GitHub push payloads only.
- Parse task references from commit messages:
  - numeric task id
  - `AF-<id>` style reference
- For matched commits:
  - create `Commit` records with status `Pending`
  - link them to the task
  - resolve author to a local user if possible, otherwise use configured fallback demo user
- Return `200` even when no task matches.
- Validate webhook signature when secret is configured; allow local-dev bypass via config.
- Add commit workflow endpoints:
  - `GET /api/tasks/{id}/commits`
  - `GET /api/commits/pending`
  - `PUT /api/commits/{id}/approve`
  - `PUT /api/commits/{id}/reject`
- Restrict approve/reject to `Admin` and `TeamLead`.
- Rejection requires a reason DTO field.
- Add local email notification support using SMTP and a local catcher such as `Mailpit`.
- Add `IEmailService` and local SMTP implementation.
- Add `Email` configuration:
  - host
  - port
  - from address
  - from name
  - optional username/password
  - SSL flag
- Send local email notifications for:
  - task assigned
  - auto-assigned task
  - commit linked
  - commit approved
  - commit rejected

**Acceptance**
- GitHub webhook creates pending commits when task references match.
- Approve/reject updates commit state and creates alerts.
- Local mail catcher receives the selected notification emails.

## Phase 7: Frontend MVP
- Scaffold or restore the real React/Vite/Tailwind app in `frontend/`.
- Build API client first:
  - base URL from env
  - access token attach
  - refresh-token retry flow on `401`
  - logout on refresh failure
- Build views in this order:
  - login/register
  - workspaces page
  - projects page
  - sprint backlog page
  - board page
  - task detail modal
  - notifications drawer
  - approvals page
- Frontend behaviors:
  - protected routes
  - role-aware control visibility
  - board drag-and-drop using task move endpoint
  - task modal for title/description/status/priority/due date edits
  - assignees section
  - dependencies section
  - comments section
  - commits section
  - activity log section
  - polling-based notifications badge/drawer
- Keep UI scope practical:
  - no SignalR
  - no email management UI
  - no analytics dashboard beyond sprint progress and notifications

**Acceptance**
- A manager can create project structures and manage the board from the UI.
- A developer can update assigned tasks from the UI.
- Notifications and approvals are visible and usable in the app.

## Phase 8: Demo Hardening and Local Delivery
- Add seed/demo data path:
  - admin
  - team lead
  - multiple developers
  - workspace
  - project
  - planning sprint
  - active sprint
  - seeded board
  - tasks with mixed states, dependencies, comments, commits, notifications
- Add concise setup and demo docs:
  - local run steps
  - webhook test steps
  - email catcher test steps
  - acceptance flow for presentation
- Make sure the demo environment can be brought up locally with concise setup steps.

**Acceptance**
- Local setup steps produce a demo-ready environment.
- The presentation flow can be run without manual data setup.

## Public APIs and Types to Add
- Board DTOs and board response models
- Dependency request/response DTOs
- Activity log DTOs
- Auto-assign request/summary DTOs
- Comment DTOs
- Notification DTOs
- Commit list / approve / reject DTOs
- GitHub webhook payload model or mapped internal model
- Email settings options class

## Test Plan
- Authorization:
  - non-members cannot access project, sprint, board, or task resources
  - only managers can mutate board structure, auto-assign, and approve/reject commits
- Board:
  - one board per project if that rule is enforced
  - default columns are seeded
  - board fetch returns grouped tasks
- Dependencies:
  - self, duplicate, cross-project, and cycle cases are rejected
- Activity:
  - tracked task mutations produce activity entries
- Auto-assign:
  - blocked tasks skipped
  - already-assigned tasks skipped
  - only developers selected
  - deterministic tie-breaking
- Comments and notifications:
  - comment permission rules hold
  - notification read and read-all work correctly
- GitHub and commits:
  - matched webhook creates pending commit
  - unmatched webhook still returns `200`
  - signature validation works when enabled
  - approval/rejection updates commit state
- Email:
  - configured local SMTP sends to catcher
  - email failure does not crash the API request path if notification persistence succeeds
- Frontend:
  - login/refresh flow works
  - board moves persist
  - modal saves update task data
  - notifications poll and render correctly

## Assumptions
- Backend remains monolithic and EF Core-based.
- In-app notifications are mandatory; email is additional delivery only.
- Email is for a local presentation workflow, not production-grade delivery.
- GitHub is the only VCS provider in scope.
- Polling remains the MVP notification transport.
- Existing package/version cleanup is part of early stabilization and should be completed before demo hardening.
