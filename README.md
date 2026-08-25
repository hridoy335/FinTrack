# FinTrackCore

Backend API for **FinTrack** — personal financial account tracking.

Built with ASP.NET Core and Clean Architecture.

## Projects

| Project | Role |
|---------|------|
| Domain | Business entities and rules |
| Application | Use cases and interfaces |
| Infrastructure | Database and external services |
| Api | HTTP endpoints and Swagger |

```
Api → Application / Infrastructure → Domain
```

## Getting started

```bash
dotnet restore
dotnet run --project src/FinTrackCore.Api
dotnet test
```

- App: `http://localhost:5027`
- Swagger: `/swagger`
