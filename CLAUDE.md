# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**RuptureRPG** — web app for managing Ruptura RPG campaigns and character sheets.
GitHub: https://github.com/Matheus-Cabral/RuptureRPG

Two goals: (1) a reliable, responsive, visually pleasant campaign management system; (2) a portfolio project demonstrating clean .NET architecture.

---

## Commands

```bash
# Development (local, no Docker)
dotnet build                         # build all projects
dotnet test                          # run all tests
dotnet test tests/Ruptura.UnitTests  # unit tests only
dotnet test tests/Ruptura.IntegrationTests  # integration tests only

# Docker (production-like)
make up        # build images and start all containers
make down      # stop and remove containers
make restart   # restart containers
make build     # rebuild images without cache
make logs      # tail all container logs
make logs-api  # tail API logs only
make migrate   # apply EF Core migrations inside container
make test      # run all tests (dotnet test)
make clean     # remove containers, volumes, and local images

# EF Core migrations (local)
dotnet ef migrations add <Name> \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API

dotnet ef database update \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

**First-time setup:**
```bash
cp .env.example .env   # fill in secrets before running make up
make up                # starts db + api + web containers
```

---

## Architecture

**Clean Architecture** — strict dependency rule: outer layers depend on inner, never the reverse.

```
Ruptura.Domain          ← pure entities, no framework dependencies
Ruptura.Application     ← use cases, interfaces, Result<T>, FluentValidation
Ruptura.Infrastructure  ← EF Core, Identity, JWT, repositories (implements Application interfaces)
Ruptura.Shared          ← DTOs shared between API and Web (requests/responses)
Ruptura.API             ← ASP.NET Core controllers, middleware, Program.cs
Ruptura.Web             ← Blazor WASM standalone (served by nginx)
```

**Dependency graph:**
```
Domain ← Application ← Infrastructure ← API
                                       ↗
                         Shared ←─────
                                  ↘
                                   Web
```

`Ruptura.Infrastructure` has a `<FrameworkReference Include="Microsoft.AspNetCore.App" />` because it uses ASP.NET Core Identity in a class library.

---

## Key Design Decisions

**Authentication flow:**
- JWT access token (15 min) + Refresh token (7 days) stored in `ApplicationUser`
- `JwtService` (Infrastructure) generates/validates tokens
- Blazor stores tokens in `localStorage` via `Blazored.LocalStorage`
- `JwtAuthStateProvider` reads the token and exposes `AuthenticationState`

**Player registration requires an invite code:**
- GM generates `InviteCode` (has `ExpiresAt`, single-use)
- `InviteCode.IsValid()` checks `!IsUsed && ExpiresAt > UtcNow`
- Player registers via `POST /api/auth/register/player` with `RegisterPlayerRequest.InviteCode`

**Runtime config for Blazor WASM:**
- `wwwroot/config.json` contains `"ApiBaseUrl": "${API_BASE_URL}"` with a shell placeholder
- nginx container runs `entrypoint.sh` which calls `envsubst` to replace the placeholder at startup
- `Program.cs` fetches `config.json` before building the DI container

**Result pattern:**
- `Result` / `Result<T>` in `Ruptura.Application.Common` — all service methods return these
- Never throw business exceptions across layer boundaries

**Settings binding:**
- `JwtSettings` is bound via `configuration.GetSection(nameof(JwtSettings))` → key in `appsettings.json` must be `"JwtSettings"` (not `"Jwt"`) — and any env var override in `docker-compose.yml` must use the matching `JwtSettings__*` prefix (not `Jwt__*`), or ASP.NET Core's config binder silently never sees it and falls back to the `appsettings.json` default. This exact mismatch shipped once (`Jwt__AccessTokenExpirationMinutes` in `docker-compose.yml` vs. the `JwtSettings` section the code reads), so `.env`'s `JWT_ACCESS_EXPIRY_MINUTES` was ignored and every access token used the 15-minute default regardless of what `.env` said. Fixed in `docker-compose.yml`; if a JWT-related env var is ever added, double-check its prefix against the section name.

**Autosave and concurrency (character sheets vs. guild sheets):**
- Both `CharacterSheetEditor.razor` and `GuildSheet.razor` autosave ~1.5s after the last edit via `AutosaveWatcher` (`Services/`), independent of the manual Save button.
- `GuildSheet` has an optimistic-concurrency version/xmin token (`UpdateGuildSheetRequest.Version`) — a save conflict shows a non-destructive banner instead of overwriting the user's edit.
- `CharacterSheet` has **no concurrency token at all**. A GM can fully edit a player's whole sheet (`GmCharacterSheet.razor` passes `CanEditStatus="true"` to the same `CharacterSheetEditor`, not just the dead/retired toggles) — so a GM and the owning player editing the same sheet at the same time is a real last-write-wins collision, silently, with autosave making it far more likely to actually happen than it was with manual-only saves. Known, accepted gap — fixing it properly means giving `CharacterSheet` the same version-token mechanism `GuildSheet` already has, which hasn't been built.

**Design system (`src/Ruptura.Web/wwwroot/css/app.css`):**
- CSS custom-property tokens drive the whole visual language — colors, spacing, the type scale (`--text-2xs` through `--text-3xl`). `--text-2xs` (11px) is the floor; never introduce a smaller font size.
- `ToastService`/`ConfirmService` (DI-scoped, `Services/`) + `ToastContainer`/`ConfirmDialog` (`Layout/`, mounted once in `MainLayout`) are the app-wide feedback pattern — inject and call `Toast.Success/Error(...)` or `await Confirm.AskAsync(...)` instead of building bespoke alert/dialog UI.
- Reusable non-layout components (search box, loading states, breadcrumbs) live in `Shared/`.
- `.ledger-table.stack-mobile` turns a table into a stacked mobile card view, but requires a `data-label="..."` attribute on every `<td>` matching its column header.
- `Breadcrumbs` (`Shared/`) — first live usage on `GmCampaignDetail.razor`: resolve the parent entity's display name via whatever list/`GetMineAsync`-style endpoint already exists (no new API), do it once in `OnInitializedAsync` (not inside a method that reruns on every write), and fall back to a sensible existing string if the name can't be resolved.
- `TableSearchBox`/`TableFilter` — client-side search over an already-loaded list; give the filtered-to-zero-results empty state its own resx string (`*.NoResults`), distinct from the "no data at all" empty state (`*.Empty`) — reusing the latter for both produces a false claim when a search matches nothing on a populated list.

---

## Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 Web API, Serilog, Swashbuckle/Swagger |
| Auth | ASP.NET Core Identity + JWT Bearer + Refresh Token |
| ORM | Entity Framework Core 8 + Npgsql (PostgreSQL) |
| Frontend | Blazor WebAssembly 8 (standalone), Blazored.LocalStorage |
| Containers | Docker + nginx 1.27-alpine |
| Tests | xUnit, Moq, FluentAssertions, Bogus, Testcontainers.PostgreSql |

---

## Testing Approach (TDD)

Write the test first, then implement. Projects:

- **`Ruptura.UnitTests`** — domain entities, application services, validators. Uses Moq + FluentAssertions + Bogus.
- **`Ruptura.IntegrationTests`** — API controllers against a real PostgreSQL instance spun up by Testcontainers. Uses `WebApplicationFactory<Program>` + Testcontainers.PostgreSql.

Integration tests use `public partial class Program;` at the end of `Ruptura.API/Program.cs` to expose the entry point to `WebApplicationFactory`.

---

## Domain Entities (src/Ruptura.Domain/Entities)

| Entity | Purpose |
|---|---|
| `InviteCode` | Single-use, time-limited invite generated by GM for player registration |
| `CharacterSheet` | Assigned to a player by the GM; player sees only their own |
| `GuildSheet` | Shared sheet for a set of players selected by the GM |
| `GuildMembership` | Join table linking a player (Guid) to a GuildSheet |

`ApplicationUser` (Infrastructure) extends `IdentityUser<Guid>` and adds `DisplayName`, `Role` (`UserRole` enum), and `RefreshToken` / `RefreshTokenExpiresAt`.

---

## Environment Variables (.env)

Key variables and what they control:

| Variable | Used by |
|---|---|
| `POSTGRES_*` | PostgreSQL container and API connection string |
| `JWT_SECRET_KEY` | Token signing (min 32 chars for HMAC-SHA256) |
| `JWT_ISSUER` / `JWT_AUDIENCE` | Token validation |
| `API_BASE_URL` | Injected into Blazor `config.json` by `entrypoint.sh` |
| `CORS_ALLOWED_ORIGIN` | API CORS policy — must match the Web container URL |
| `API_PORT` / `WEB_PORT` | Host port bindings in docker-compose |

---

## RPG System Context (docs/)

The `docs/` folder contains the full Ruptura RPG system design:

- `docs/GDD_Ruptura.md` — authoritative game design document (1,500+ lines, Brazilian Portuguese)
- `docs/manuais/` — Player and GM manuals
- `docs/fichas/` — PDF character/guild/creature/NPC sheets

When implementing UI features (character sheets, guild sheets), the GDD is the source of truth for fields and rules. The GDD's 16 Design Principles define what the software must support — do not add features that contradict them.
