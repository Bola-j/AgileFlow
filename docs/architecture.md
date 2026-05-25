# Architecture

AgileFlow is a simple monolithic application with a layered backend:

- **API**: HTTP endpoints, authentication, and request/response models.
- **Core**: Entities, DTOs, and interfaces.
- **Infrastructure**: EF Core data access and implementations.

Frontend is a standalone React app consuming the API over HTTP.
