# RuptureRPG

A web app for running **Ruptura**, a hardcore dungeon-crawler tabletop RPG — campaign management,
character sheets, guild sheets, and Game Master tooling, built as a full .NET 8 / Blazor
WebAssembly application.

Two goals shaped this project: (1) a reliable, responsive campaign manager a real group could run
sessions with, and (2) a portfolio piece demonstrating Clean Architecture end-to-end in a
production-shaped .NET codebase.

## What it does

- **Campaigns & rosters** — a Game Master runs campaigns, invites players via single-use codes,
  and manages the party roster.
- **Character sheets** — an 11-module sheet per player (identity, attributes, skills, combat,
  talents, spells, techniques, equipment, journal with media uploads, and more), with fully
  calculated derived stats.
- **Guild sheet** — a shared, campaign-wide sheet all players can read and edit: prestige,
  influence, resources, buildings, staff, research/crafting, doctrines, and an interlude
  (time-advance) calculator.
- **Homebrew catalog** — GM-editable catalog of Origins, Backgrounds, Lineages, Skills, Spells,
  Techniques, Equipment, Installations, and Doctrines, with schema-driven per-type forms plus a
  raw-JSON escape hatch for anything the forms don't cover yet.
- **GM tools** — a bestiary/NPC library, an encounter generator (power-level math, threat rating,
  pressure scaling), a reward planner, an in-session combat tracker, and campaign prep tools
  (arcs, floors, session logs).
- **Campaign dashboard** — current floor, dungeon pressure, active party, and pending
  rank-promotion notifications at a glance.
- **In-app manuals** — the Player and GM manuals, rendered from Markdown, in whichever language
  (English or Portuguese) the app is currently set to.
- **Bilingual throughout** — every UI string, error message, and manual page is available in
  English and Brazilian Portuguese.

## Tech stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 Web API, Serilog, Swashbuckle/Swagger |
| Auth | ASP.NET Core Identity + JWT Bearer + refresh tokens |
| ORM | Entity Framework Core 8 + Npgsql (PostgreSQL) |
| Frontend | Blazor WebAssembly 8 (standalone), Blazored.LocalStorage |
| Containers | Docker + nginx 1.27-alpine |
| Tests | xUnit, Moq, FluentAssertions, Bogus, Testcontainers.PostgreSql |

## Architecture

Strict Clean Architecture — outer layers depend on inner layers, never the reverse:

```
Ruptura.Domain          ← pure entities, no framework dependencies
Ruptura.Application     ← use cases, interfaces, Result<T>, FluentValidation
Ruptura.Infrastructure  ← EF Core, Identity, JWT, repositories
Ruptura.Shared          ← DTOs shared between API and Web (zero project references)
Ruptura.API             ← ASP.NET Core controllers, middleware
Ruptura.Web             ← Blazor WebAssembly (standalone, served by nginx)
```

Every service method returns a `Result` / `Result<T>` — no business exceptions cross layer
boundaries. Every mutation is validated and authorized server-side; the client never gets to
trust its own computed values (power levels, thresholds, derived stats are always
server-recomputed).

## Getting started

### Docker (recommended)

```bash
cp .env.example .env   # fill in secrets
make up                 # builds images, starts db + api + web
```

The web app comes up on `WEB_PORT` (default in `.env.example`), the API on `API_PORT`.

Other useful targets: `make down`, `make restart`, `make logs`, `make logs-api`, `make migrate`,
`make test`, `make clean`. See the [Makefile](Makefile) for the full list.

### Local development (no Docker)

Requires the .NET 8 SDK and a local PostgreSQL instance.

```bash
dotnet build
dotnet test                                    # full suite
dotnet test tests/Ruptura.UnitTests            # unit only
dotnet test tests/Ruptura.IntegrationTests     # integration only (spins up Postgres via Testcontainers)

dotnet ef database update \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

## Documentation

- [`docs/GDD_Ruptura.en.md`](docs/GDD_Ruptura.en.md) — the full Ruptura game design document
  (English). [`docs/GDD_Ruptura.md`](docs/GDD_Ruptura.md) has the original Portuguese.
- [`docs/manuais/`](docs/manuais) — Player and GM manuals, in both languages (also available
  in-app once you're logged in, under **Manuals**).
- [`docs/fichas/`](docs/fichas) — printable character/guild/creature/NPC sheet PDFs.
- [`CLAUDE.md`](CLAUDE.md) — repo conventions, commands, and architectural notes for contributors.
