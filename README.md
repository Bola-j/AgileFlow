# AgileFlow

AgileFlow is a lightweight Agile project/task management system inspired by Jira. This repository contains a simple, monolithic scaffold with a .NET 8 API backend and a React + Vite frontend.

## Structure

```
backend/        .NET 8 Web API + Core + Infrastructure
frontend/       React + TypeScript + Vite + Tailwind
docs/           Project documentation
```

## Quick Start (Local)

1. Run SQL Server.
2. Start backend: `dotnet run --project backend/API/API.csproj`
3. Start frontend: `npm install && npm run dev` from `frontend/`

## Notes

This is a scaffold only: minimal endpoints and no business logic.
