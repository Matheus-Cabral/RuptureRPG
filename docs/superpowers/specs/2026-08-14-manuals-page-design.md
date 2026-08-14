# Manuals Page — Design Spec

**Date:** 2026-08-14
**Status:** Approved (in-chat design), pending written self-review + user sign-off before planning.

## Context

RuptureRPG is bilingual (pt-BR + en). `docs/manuais/Manual_do_Jogador.md` (Player Manual) and
`docs/manuais/Manual_do_Mestre.md` (GM Manual) are the player/GM-facing rulebooks — separate from
`docs/GDD_Ruptura.md`, which is the internal design-authority document and is NOT part of this
feature (stays repo-only; translated as a parallel, independent effort).

The user wants the two manuals available inside the app itself:
- A **GameMaster** account sees both manuals (GM Manual + Player Manual).
- A **Player** account sees only the Player Manual.
- The language shown must follow the app's existing language selector (`LanguageSwitcher.razor`),
  not a separate picker.

This is new surface area (no prior in-app document viewer), so it is scoped as its own
architectural unit rather than folded into the translation or README work.

## Non-goals

- No markdown *editing* UI — this is read-only, rendering files that ship with the app.
- No GDD viewer in-app (explicitly out of scope, confirmed with user).
- No per-manual language picker — language is inherited from the existing global switcher only.
- No bUnit coverage — the project's UI testing convention is "build + manual verification"
  (established and reconfirmed during the 2026-08-14 cleanup track); only the pure mapping helper
  gets a unit test.

## Content source of truth & build pipeline

`docs/manuais/*.md` remain the only edited copies (git history, PR diffs, etc. all live there).
Four files feed the app:

| File | Language |
|---|---|
| `docs/manuais/Manual_do_Jogador.md` | pt-BR (existing, unchanged) |
| `docs/manuais/Manual_do_Jogador.en.md` | en (new, translation sub-project) |
| `docs/manuais/Manual_do_Mestre.md` | pt-BR (existing, unchanged) |
| `docs/manuais/Manual_do_Mestre.en.md` | en (new, translation sub-project) |

`src/Ruptura.Web/Ruptura.Web.csproj` gains four `<Content Include="..\..\docs\manuais\...">` items,
each with a `Link="wwwroot\content\manuals\<filename>"` and
`CopyToOutputDirectory="PreserveNewest"` / `CopyToPublishDirectory="PreserveNewest"`. This copies
the four files into `wwwroot/content/manuals/` at build/publish time — no manual duplication, no
new sync step to forget. nginx (or the dev server) then serves them as ordinary static assets,
exactly like `config.json` today.

## Fetching

New `IManualClientService` / `ManualClientService` in `Ruptura.Web/Services/`, following the
project's established `I*ClientService` pattern (one interface + implementation per feature,
injected into pages — see `INotificationClientService` for the shape to mirror). Unlike the other
client services, this one does NOT call the API: it wraps a dedicated named `HttpClient`
registered in `Program.cs` with `BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)` (the
app's own origin — the same base the existing `bootstrapHttp` throwaway client already resolves
`config.json` against), not the `"RupturaApi"` client.

```csharp
public interface IManualClientService
{
    Task<string?> GetManualAsync(ManualType type, CancellationToken ct = default);
}
```

`GetManualAsync` resolves the file name via `ManualReference.FileNameFor(type,
CultureInfo.CurrentUICulture.Name)`, issues `GetStringAsync("content/manuals/{fileName}")`, and
returns `null` on any `HttpRequestException` (the page shows an inline error state — see below).
No `Result<T>` wrapper: this isn't a backend call with business error codes, it's a static-asset
fetch with exactly one failure mode.

## Pure, testable mapping

```csharp
// Ruptura.Web/Content/ManualReference.cs
public enum ManualType { Player, GameMaster }

public static class ManualReference
{
    public static string FileNameFor(ManualType type, string culture)
    {
        var baseName = type switch
        {
            ManualType.Player => "Manual_do_Jogador",
            ManualType.GameMaster => "Manual_do_Mestre",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        // LanguageSwitcher only ever stores exactly "en" or "pt-BR" (see Layout/LanguageSwitcher.razor) —
        // match that literally rather than parsing/normalizing a general BCP-47 tag.
        var suffix = string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase) ? ".en" : "";
        return $"{baseName}{suffix}.md";
    }
}
```

Unit-tested directly (4 cases: Player×pt-BR, Player×en, GameMaster×pt-BR, GameMaster×en) —
mirrors the project's existing pattern of extracting pure logic into a static class with direct
unit coverage (`DungeonPressure.StateFor`, `CombatOrder`, etc.). Lives in `Ruptura.Web` (already
referenced by `Ruptura.UnitTests`), not `Ruptura.Shared`, since it has no API/backend consumer.

## Page

New `src/Ruptura.Web/Pages/Manuals.razor`:

- `@page "/manuals"`, `@attribute [Authorize]` (any authenticated user — both roles reach the
  page; content is gated *inside* it, not by a role-restricted route).
- `<AuthorizeView Roles="GameMaster">`: renders a `nav nav-tabs` with two tabs ("GM Manual" /
  "Player Manual" — same active-tab-toggle pattern as `CharacterSheetEditor.razor`'s
  `_activeTab` field + `nav-link active` class), each tab lazily fetching and then caching its
  manual in a field so switching tabs doesn't re-fetch.
- `<AuthorizeView Roles="Player">`: fetches and renders the Player Manual directly, no tabs.
- Loading state: `LoadingIndicator` while the fetch for the active manual is in flight.
- Error state: if `GetManualAsync` returns `null`, an inline `alert-danger` block with a generic
  localized message (`Manuals.LoadError`) — this is a shipped static asset, so a fetch failure
  means a build/deploy problem, not a user-recoverable error; no retry button needed.
- Render: `Markdig.Markdown.ToHtml(raw, pipeline)` with
  `new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()` (covers the manuals' pipe
  tables), wrapped in `<div class="manual-content">@((MarkupString)html)</div>`.
  - The `MarkupString` cast bypasses Blazor's automatic HTML-escaping. This is safe here because
    the Markdown source is exclusively our own versioned repository content
    (`docs/manuais/*.md`, shipped as static build assets) — never user-submitted or
    database-sourced text. Same trust boundary as serving a static `.html` file.

## New package

`Ruptura.Web.csproj` gets `<PackageReference Include="Markdig" Version="1.3.2" />` (current
stable on NuGet as of this spec) — pure managed C#, no native/P-Invoke dependencies, safe under
Blazor WASM (widely used in other WASM doc-viewer projects for this exact reason).

## Styling

New `.manual-content` block in `app.css`, near the other content-block rules, using existing
tokens only (`--font-body`, `--text-*` scale, `--border`, `--bg-surface`, spacing scale already
in use elsewhere) — headings get the existing type-scale steps, `<table>` gets the same
`.ledger-table`-adjacent border treatment (thin `--border` rules, `--bg-surface` header row,
readable padding), `<blockquote>` gets a left accent bar in `--accent`. No new color tokens.

## Navigation

`NavMenu.razor` gets one new `<NavLink>` to `/manuals`, placed alongside `/dashboard` — outside
both `AuthorizeView Roles="Player"` / `Roles="GameMaster"` blocks (both roles see the same link;
the page itself decides what to render). New resx key `Nav.Manuals` (en: "Manuals", pt-BR:
"Manuais").

## i18n

New resx keys (both `AppStrings.resx` and `AppStrings.pt-BR.resx`, kept in parity per the
project's `count(en) == count(pt-BR)` convention):

| Key | en | pt-BR |
|---|---|---|
| `Nav.Manuals` | Manuals | Manuais |
| `Manuals.Title` | Manuals | Manuais |
| `Manuals.Tab.GameMaster` | GM Manual | Manual do Mestre |
| `Manuals.Tab.Player` | Player Manual | Manual do Jogador |
| `Manuals.LoadError` | Could not load this manual. Please try again later. | Não foi possível carregar este manual. Tente novamente mais tarde. |

## Testing

- `ManualReference` pure mapping: 4 unit test cases (both types × both cultures), in
  `Ruptura.UnitTests`.
- Everything else (fetch/render/tabs/role-gating) is build + manual browser verification, per the
  project's established UI-testing convention — no bUnit.
- Full existing unit + integration suites must stay green (no backend/API touched by this
  feature at all — purely additive Web-layer + static content).

## Risks / open notes

- If Markdig's `UseAdvancedExtensions()` output for a table or nested list looks off against the
  `.manual-content` CSS on first manual QA pass, that's an expected fit-and-finish iteration, not
  a design gap — flagged here so it isn't mistaken for scope creep during implementation.
- The English manuals (`*.en.md`) are produced by the parallel translation sub-project; this plan
  does not implement or verify their content, only the delivery pipeline for whichever files
  exist at `docs/manuais/*.md` / `*.en.md` at build time.
