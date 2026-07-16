# AgileFlow

AgileFlow is a workspace-based Agile project management application for software teams. It brings authentication, workspace roles, project and sprint planning, Kanban task management, dependency tracking, submission and review, activity history, dashboards, and workflow email into one system.

The repository contains:

- a .NET 8 ASP.NET Core Web API;
- a React 19, TypeScript, and Vite frontend;
- SQL Server persistence through Entity Framework Core 8;
- Postman/Newman API workflow assets;
- two Playwright browser scenarios;
- Dockerfiles and a development Docker Compose stack.

## Architecture

The backend is organized as Onion Architecture with the Repository Pattern:

```text
API
+-- references Application
`-- references Infrastructure

Infrastructure
+-- references Application
`-- references Domain

Application
`-- references Domain

Domain
`-- references ASP.NET Identity EF package
```

Project responsibilities:

- `Domain`: entities, enums, and entity state methods.
- `Application`: DTOs, interfaces, repository/service contracts, and AutoMapper profiles.
- `Infrastructure`: EF Core, repositories, service implementations, authentication tokens, SMTP email, and the reminder worker.
- `API`: controllers, middleware, dependency injection, authentication, Swagger, static files, and startup.

Current Onion implementation notes:

- `AppUser` in Domain inherits `IdentityUser`.
- Business service implementations are located in Infrastructure.
- API directly references Infrastructure.
- Repositories call `SaveChangesAsync` independently instead of using one use-case transaction.

## Implemented Features

- Registration with email-confirmation flow.
- Confirmed-user login with JWT access and refresh tokens.
- Refresh-token rotation and logout revocation.
- Account profile read/update and profile-picture upload.
- Workspace creation and workspace-scoped roles.
- Workspace member add, restore, role update, removal, and detail read.
- Project create, read, update, and soft delete.
- Sprint create, read, update, start, complete, and progress calculation.
- One board per project with ordered columns.
- Task creation, field editing, status changes, board movement, assignment, and soft delete.
- Commit-hash submission and Admin/TeamLead review.
- Task dependencies with self, duplicate, cross-project, and cycle checks.
- Task activity logs.
- Dashboard summary and assigned-task endpoints.
- SMTP email attempts for verification, workspace invitation, assignment, submission, review decisions, and due reminders.
- Email success/failure audit attempts.
- Automatic EF migration at API startup.

Not implemented as complete features:

- Google/GitHub third-party authentication. This piece is assigned to Moataz Hamdy (`M3tazz`) and is planned but not merged in the submitted repository snapshot.
- GitHub commit verification or webhooks.
- General threaded task discussion.
- A user-facing in-app notification API or inbox.
- Automatic task distribution.
- Production deployment infrastructure.

## Workspace Permissions

| Capability | Admin | TeamLead | Developer |
| --- | :---: | :---: | :---: |
| Read workspace resources | Yes | Yes | Yes |
| Update or delete a workspace | Yes | Yes | No |
| Add, restore, remove, or change members | Yes | No | No |
| Manage projects, sprints, board columns, and tasks | Yes | Yes | No |
| Assign users and manage dependencies | Yes | Yes | No |
| Review task submissions | Yes | Yes | No |
| Edit an assigned task's fields | Yes | Yes | Yes |
| Submit an assigned task | Yes | Yes | Yes |
| Move a task or directly change its status | Yes | Yes | No |

Any authenticated user can create a workspace. The creator becomes its Admin.

## Repository Structure

```text
backend/
  API/              Controllers, middleware, startup, Swagger, static files
  Application/      DTOs, interfaces, contracts, AutoMapper profiles
  Domain/           Entities, enums, base entity
  Infrastructure/   EF Core, migrations, repositories, services, email worker
  Tests/            Separate xUnit project with one placeholder test

frontend/
  src/app/          Providers and route composition
  src/features/     Auth, account, workspace, project, sprint, board, task
  src/components/   Shared UI components
  src/services/     Axios client, authentication storage, dashboard API

postman/            Postman collection and local environment
tests/e2e/          Playwright scenarios
.github/workflows/  Backend solution build workflow
docs/               Project documentation and backend technical report
```

`backend/Tests/Tests.csproj` is not included in `backend/AgileFlow.slnx`. The current CI solution build therefore does not compile or run it.

## Documentation

- [Project Documentation](docs/AgileFlow-project-documentation.md) — scope, requirements, roles, design, implementation, verification, limitations, and user guidance.
- [Backend Technical Report](docs/AgileFlow-backend-discussion-report.md) — architecture, runtime behavior, authentication, authorization, persistence, task workflow, security, API coverage, and engineering improvements.

## Local Development

Prerequisites:

- .NET 8 SDK
- Node.js 20 or newer
- SQL Server reachable through the configured connection string
- optional SMTP test server on the configured host and port

Run the API:

```powershell
dotnet run --project backend/API/API.csproj
```

The checked-in development launch profile uses:

- API: `http://localhost:6358`
- SQL Server: `Server=localhost`
- database: `AgileFlow_Development`
- SMTP: `localhost:2525`
- frontend confirmation URL: `http://127.0.0.1:5173`

The API calls `Database.Migrate()` during startup.

Run the frontend:

```powershell
cd frontend
npm install
npm run dev
```

The frontend runs on `http://127.0.0.1:5173` and defaults to `http://localhost:6358` for API requests.

## Docker Compose

The repository includes SQL Server, API, and frontend services:

```powershell
docker compose up --build
```

The Compose stack is for local development:

- API environment is `Development`.
- Swagger, development CORS, and the hidden development confirmation endpoint are enabled.
- SMTP is not included.
- `Jwt__AccessTokenMinutes` is configured, but the backend reads `Jwt:ExpiryMinutes`.
- `Jwt__RefreshTokenDays` is configured, but refresh lifetime is hard-coded to seven days.

## Verification

```powershell
dotnet build backend/AgileFlow.slnx -c Release
dotnet test backend/Tests/Tests.csproj -c Release

cd frontend
npm run lint -- --format stylish
npm run build
cd ..

npm run postman:test
npm run e2e
```

Requirements:

- Newman requires the API and SQL Server to be running.
- Playwright requires the frontend and API.
- The authenticated Playwright scenario requires `PLAYWRIGHT_CONFIRMED_EMAIL`.

Repository verification completed on 16 July 2026:

- backend solution build passed with one `NU1902` warning for MailKit `4.7.1.1`;
- the separate xUnit project passed its one placeholder test;
- frontend lint passed;
- frontend build passed with a large-bundle warning;
- Newman and Playwright were not run during the documentation rewrite.

## Current Scope and Improvement Areas

- The xUnit project provides no meaningful business-rule coverage.
- CI only restores and builds the four-project backend solution.
- Developer board filtering is not applied consistently to other task-read endpoints.
- Workspace soft deletion does not soft-delete all child resources.
- Project creation and several task workflows use multiple independent saves.
- Task status is inferred from mutable column names.
- A Pending task can be moved to a non-Done column, clearing approval without resolving its Pending commit.
- An already Done/Approved task can be submitted again.
- Dependency regression does not reopen already completed dependent tasks.
- Activity logs allow 500-character values while descriptions and review comments allow 2,000 characters.
- Undefined numeric role/status/priority enum values are not consistently rejected.
- Cancelled tasks are included in the due-reminder query.
- The confirmation response exposes different email values for unknown and existing user ids.
- Any workspace member can read another member's detailed profile response.
- Profile-picture static files are registered before HTTPS redirection.
- Swagger currently displays the stale title `SolKey API v1`.

## Team Contributions

| Contributor | Contribution basis | Responsibility |
| --- | --- | --- |
| Bola Ghaly | Areas visible in Git history | Setup, JWT, sprint/task services, authorization, React frontend, CI, Postman, integration, and documentation |
| Fatma Elkassaby | Areas visible in Git history | Domain and EF setup, membership, boards, dependencies, and activity logs |
| Youssef-Amer17 / Youssef Amer | Areas visible in Git history | Initial frontend, authentication/profile, email verification, and notification audit |
| Yahia Hany | Areas visible in Git history | Workspace and project backend |
| Moataz Hamdy (`M3tazz`) | Team-assigned responsibility | Google/GitHub third-party authentication; planned and not merged in the submitted repository snapshot |

The first four summaries are based on repository history and commit subjects. Moataz's responsibility is recorded from the team's assignment and is presented transparently as planned work.
