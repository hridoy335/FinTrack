# FinTrackCore

Backend API for **FinTrack** — personal financial account tracking.

Built with ASP.NET Core and Clean Architecture.

## Related project

| Project | Path | Role |
|---------|------|------|
| **FinTrackCore** (this repo) | `D:\Hridoy\FinTrack` | Backend API |
| **FinTrack Client** | `D:\Hridoy\FinTrack_Client` | Angular frontend |

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

### With the Angular client

```bash
# Terminal 1 — API (from this repo)
dotnet run --project src/FinTrackCore.Api

# Terminal 2 — client
cd D:\Hridoy\FinTrack_Client
npm start
```

CORS allows `http://localhost:4200` (see `Cors:AllowedOrigins` in `appsettings.json`).

- App: `http://localhost:5027`
- Swagger: `/swagger`
- Client: `http://localhost:4200`
