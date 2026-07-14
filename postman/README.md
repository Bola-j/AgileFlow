# AgileFlow Postman Suite

This folder contains the holistic Postman/Newman test suite for the AgileFlow Web API.

## Prerequisites

1. Run SQL Server with the API connection string configured.
2. Start the backend in Development mode:

```powershell
dotnet run --project backend/API/API.csproj
```

The default local API URL is `http://localhost:6358`. Change `baseUrl` in `AgileFlow.local.postman_environment.json` if needed.

## Run With Newman

Install dependencies once:

```powershell
npm install
```

Run the full suite:

```powershell
npm run postman:test
```

Verbose run:

```powershell
npm run postman:test:verbose
```

## Manual Postman Runner

1. Import `AgileFlow.postman_collection.json`.
2. Import `AgileFlow.local.postman_environment.json`.
3. Select the `AgileFlow Local` environment.
4. Run the collection from top to bottom.

## Data Strategy

The suite is non-destructive. It creates unique timestamped users, workspace, project, sprint, columns, and tasks on each run. It does not reset or clean the database.

## Coverage

The collection covers auth, account profile, workspaces and members, projects, sprints, boards, task CRUD, dependencies, activity logs, board visibility by role, and representative exception middleware responses.
