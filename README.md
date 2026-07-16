# AgileFlow

AgileFlow is an Agile project management platform for workspace-based teams. It supports authenticated workspaces, projects, sprints, Kanban boards, task assignment, dependency tracking, submit/review approval flow, dashboard summaries, account profiles, email verification, and workflow email notifications.

The current application is a .NET 8 API backend with a React 19 + TypeScript + Vite frontend. The backend owns the business rules for workspace roles, task approval, dependency validation, sprint completion, email audit logs, and database migrations.

## Main Features

- JWT authentication with refresh tokens and email verification.
- Account profile editing and upload-only profile pictures.
- Workspace, member, project, sprint, board, and task management.
- Role-based permissions:
  - Admin: member management plus delivery management.
  - TeamLead: project, sprint, board, task, assignment, dependency, and review management.
  - Developer: assigned task editing and submit-for-review.
- Task lifecycle with commit submission, approval/rejection comments, dependency gates, and activity logs.
- Sprint progress and dashboard aggregate APIs.
- SMTP-backed email notifications with audited delivery logs and due-date reminders.
- React dashboard UI with TanStack Query, Axios, React Hook Form, Zod, dnd-kit, Recharts, and Tailwind.
- Postman/Newman and Playwright workflow coverage.

## Repository Structure

```text
backend/
  API/              ASP.NET Core controllers, middleware, startup, auth, static upload serving
  Application/      DTOs, interfaces, mapping profiles, service contracts
  Domain/           Entities, enums, domain state
  Infrastructure/   EF Core DbContext, migrations, repositories, services, email worker
  Tests/            Backend test project

frontend/
  src/app/          Providers and route composition
  src/features/     Feature modules for auth, workspace, projects, board, tasks, sprints, account
  src/components/   Shared UI primitives and reusable app components
  src/services/     Axios API client and cross-feature API helpers

postman/            API workflow collection and local environment
tests/e2e/          Playwright E2E scenarios
docs/               Architecture and project documentation
```

## Local Development

Prerequisites:

- .NET 8 SDK
- Node.js 20+
- SQL Server LocalDB or SQL Server
- Optional: smtp4dev or another SMTP test inbox

Backend:

```powershell
dotnet run --project backend/API/API.csproj
```

The API uses `backend/API/Properties/launchSettings.json` for local development defaults. Startup applies EF Core migrations automatically with `Database.Migrate()`.

Frontend:

```powershell
cd frontend
npm install
npm run dev
```

The frontend defaults to `VITE_API_URL=http://localhost:6358` and runs on Vite at `http://127.0.0.1:5173`.

Verification:

```powershell
dotnet build backend/AgileFlow.slnx -c Release
cd frontend
npm run lint -- --format stylish
npm run build
cd ..
npm run postman:test
npm run e2e
```

## Team Roles

The roles below are tailored from repository history and the provided team names. Git evidence comes from local Git author names, commit subjects, and touched areas.

| Team member | Git identity seen in history | Project role | Evidence from repository history |
| --- | --- | --- | --- |
| Bola Gerges Saeed Ghaly | `Bola Ghaly <bolagerges221@gmail.com>` | Team Lead, full-stack integration lead, backend workflow owner | Highest commit volume; solution setup, JWT auth, sprint/task services, workspace authorization, React frontend implementation, dashboard, Postman workflow coverage, Docker/E2E setup, task lifecycle hardening, merge/integration work. |
| Fatma Mahmoud Abdelkader Elkassaby | `Fatma Elkassaby <fatemamahmoud2004@gmailcom>` | Backend domain and board/task workflow engineer | Initial domain entities and EF configurations; workspace membership; board and column management; task dependencies and activity logging; sprint-scoped board workflow. |
| Youssef Amer Sayed Mostafa | `Youssef Amer <yousefamer771@gmail.com>`, `Youssef-Amer17 <yousefamer771@gmail.com>` | Authentication, profile, email verification/notification, and early frontend flow engineer | Initial HTML/JavaScript UI pages; login, registration, and profile management; email verification, SMTP notifications, email audit log, and due-date reminder worker. |
| Yahia Hany Shaker Rezk | `Yahia Hany <yahyahany222@gmail.com>` | Workspace and project API contributor | Workspace and project backend slice including controllers, services, repositories, interfaces, and mapping profiles. |

## Current Notes

- Registering a user sends an email verification link and does not issue tokens until the account is confirmed.
- Existing workflow rules are enforced server-side; frontend controls are convenience only and are not the source of authorization.
- Sprint completion requires every task in the sprint to be `Done` and `Approved`.
- Tasks can enter `Done` only after review approval and after dependencies are also done and approved.
- Profile pictures are uploaded to the API and served from `/uploads/profile-pictures/...`; raw URL entry is intentionally not part of the profile flow.
