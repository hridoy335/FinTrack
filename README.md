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

## Getting started (Docker — for clients)

Give the client only:

1. This repo’s `docker-compose.yml` (and optional `.env` from `.env.example`)
2. Docker Desktop installed

They do **not** need .NET, Node, or a local PostgreSQL install.

```bash
# optional: copy .env.example → .env and set passwords
docker compose up -d
```

What happens automatically:

1. Pulls `hridoy335/fintrack:latest` (API)
2. Pulls `postgres:16` (database)
3. Starts Postgres, then the API
4. API applies EF migrations on startup

| Service | URL |
|---------|-----|
| **API** | http://localhost:5027 |
| **Swagger** | http://localhost:5027/swagger |
| **Postgres** | localhost:5432 (defaults: `postgres` / `12345`, db `fintrackcore`) |

Stop:

```bash
docker compose down
```

Wipe database volume:

```bash
docker compose down -v
```

Update to the newest API image (after a new push to `main`):

```bash
docker compose pull api
docker compose up -d api
```

## Getting started (Docker — local full stack with Angular)

Clone **both** repos as siblings, then:

```bash
docker compose -f docker-compose.dev.yml up --build
```

| Service | URL |
|---------|-----|
| **App (frontend)** | http://localhost:4200 |
| **API** | http://localhost:5027 |
| **Swagger** | http://localhost:5027/swagger |

The frontend nginx proxies `/api` to the API container.

## Getting started (local without Docker)

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
cd ../FinTrack_Client
npm start
```

CORS allows `http://localhost:4200` (see `Cors:AllowedOrigins` in `appsettings.json`).

- API: `http://localhost:5027`
- Swagger: `/swagger`
- Client: `http://localhost:4200`
