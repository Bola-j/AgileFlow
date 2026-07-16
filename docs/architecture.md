# AgileFlow Architecture

AgileFlow is a layered monolithic application. The API, domain model, business services, repositories, and React frontend live in one repository, but responsibilities are separated by project and feature boundaries.

## Backend Architecture

The backend uses .NET 8, ASP.NET Core, Entity Framework Core, ASP.NET Identity, JWT authentication, and SQL Server.

```text
API -> Application -> Domain
API -> Infrastructure -> Application -> Domain
```

- `backend/API`: controllers, middleware, dependency injection, authentication setup, CORS, Swagger, startup migrations, static upload serving, and hosted worker registration.
- `backend/Application`: DTOs, service interfaces, repository interfaces, AutoMapper profiles, and contracts shared by API and Infrastructure.
- `backend/Domain`: entities and enums such as workspace roles, sprint status, task status, task priority, approval status, commit status, and email event types.
- `backend/Infrastructure`: EF Core persistence, migrations, repository implementations, business services, SMTP email delivery, notification audit logging, and due-date reminder background work.

The API layer should stay thin. Business rules belong in services, especially:

- `WorkspaceService` and `WorkspaceAuthorizationService` for workspace membership and role rules.
- `ProjectService`, `SprintService`, and `TaskService` for delivery workflow rules.
- `BoardService` for board and column rules.
- `AuthService` and email services for verification, token, and notification workflows.

## Frontend Architecture

The frontend is a React 19 + TypeScript + Vite application using React Router v7, TanStack Query, Axios, React Hook Form, Zod, Tailwind, dnd-kit, Recharts, Framer Motion, Lucide icons, and shadcn-style UI primitives.

Feature code is organized under `frontend/src/features`:

- `auth`: login, register, refresh persistence, email verification, resend confirmation.
- `workspace`: workspace list/details, member table, role-based controls.
- `projects`: project list/details, project forms, sprint entry points.
- `sprints`: sprint details, progress chart, start/complete actions, embedded board.
- `board`: Kanban columns, task cards, column ordering, task movement.
- `tasks`: task detail modal, edit, submit/review, assignees, dependencies, activity logs.
- `account`: current-user profile and profile-picture upload.

Frontend role checks only control visibility and ergonomics. Backend authorization remains the source of truth.

## Authorization Model

Workspace role controls all inner workspace operations:

| Capability | Admin | TeamLead | Developer |
| --- | --- | --- | --- |
| Add/remove members | Yes | No | No |
| Change member roles | Yes | No | No |
| Edit workspace details | Yes | Yes | No |
| Project CRUD | Yes | Yes | No |
| Sprint CRUD/start/complete | Yes | Yes | No |
| Board column CRUD/order | Yes | Yes | No |
| Task create/delete/move/status | Yes | Yes | No |
| Assign/unassign task users | Yes | Yes | No |
| Add/remove task dependencies | Yes | Yes | No |
| Approve/reject task review | Yes | Yes | No |
| Edit assigned task fields | Yes, if assigned or manager | Yes, if assigned or manager | Yes, if assigned |
| Submit assigned task for review | Yes, if assigned | Yes, if assigned | Yes, if assigned |

The workspace creator remains an Admin and cannot be demoted or removed.

## Task Lifecycle

Tasks combine status, approval, commits, dependencies, and activity logs into one workflow:

1. Admin or TeamLead creates and assigns tasks.
2. Assigned users can edit allowed fields and submit with a commit hash.
3. Submit sets approval to `Pending` and creates a pending commit record.
4. Admin or TeamLead reviews with a required comment.
5. Approval marks the commit as merged and the task as `Done` only when dependencies are also `Done` and `Approved`.
6. Rejection records the comment, marks the commit rejected, and keeps task data and assignees intact.
7. Sprint completion is blocked until every task is `Done` and `Approved`.

## Email and Notifications

Email delivery is isolated behind application interfaces and implemented in Infrastructure with SMTP.

- Registration sends an email confirmation link and blocks login until confirmation.
- Workspace membership, task assignment, task submission, review decisions, and due-date reminders can send emails.
- `EmailNotificationLogs` stores audit and deduplication data.
- Email failures are logged and audited but do not roll back the original business action.

## Persistence and Startup

EF Core migrations define schema changes. The API applies migrations on startup using `Database.Migrate()`, so local development can create or update the database automatically when the configured SQL Server is available.

Static profile-picture uploads are stored under `backend/API/wwwroot/uploads/profile-pictures/` and served by the API static-file middleware.

## Team Responsibility Map

Roles are inferred from local Git history and the names provided by the team lead:

- Bola Gerges Saeed Ghaly: Team Lead and full-stack integration owner. Owns architecture coordination, backend workflow hardening, React migration, CI/workflow validation, and final merge decisions.
- Fatma Mahmoud Abdelkader Elkassaby: Backend domain and workflow engineer. Owns core entities/configurations, board services, task dependencies, activity logs, and sprint-board workflow.
- Youssef Amer Sayed Mostafa: Auth, profile, email, and early frontend workflow engineer. Owns authentication/profile flows, email verification, SMTP notification/audit flow, and initial static UI prototypes.
- Yahia Hany Shaker Rezk: Workspace/project backend contributor. Owns early workspace and project controllers, services, repositories, interfaces, and mappings.
