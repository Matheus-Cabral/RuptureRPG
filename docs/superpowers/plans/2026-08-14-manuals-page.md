# Manuals Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an in-app `/manuals` page that shows the GM Manual + Player Manual to GameMaster accounts (tabbed) and only the Player Manual to Player accounts, rendered from Markdown in whichever language (`en`/`pt-BR`) the app's existing language switcher currently has selected.

**Architecture:** `docs/manuais/*.md` (2 manuals × 2 languages = 4 files) stay the single edited source. A `Content Include ... Link="wwwroot\content\manuals\..."` item in `Ruptura.Web.csproj` copies them into the app's static assets at build/publish time (validated empirically: the file lands in `bin/.../wwwroot/content/manuals/` AND is registered in the static-web-assets manifest, so both the dev server and the nginx-served production build serve it correctly). A same-origin `HttpClient` (new named client `"RupturaSelf"`, separate from the existing `"RupturaApi"` client) fetches the raw Markdown; a pure `ManualReference.FileNameFor(ManualType, culture)` helper (unit-tested) picks the right file; Markdig renders it to HTML, shown via `MarkupString` (safe — the Markdown is 100% our own versioned repo content, never user input). The page mirrors the codebase's existing `Dashboard.razor` → `GmDashboard`/`PlayerDashboard` role-split pattern exactly.

**Tech Stack:** Blazor WebAssembly 8, Markdig 1.3.2 (new dependency), existing `IStringLocalizer<AppStrings>` i18n, existing `AuthorizeView` role-gating.

**Spec:** `docs/superpowers/specs/2026-08-14-manuals-page-design.md`

## Global Constraints

- `docs/manuais/*.md` (pt-BR, existing) and `docs/manuais/*.en.md` (new, produced by a separate parallel translation effort — may or may not exist yet when this plan is implemented; the pipeline must not break either way) are the ONLY source of manual content. Never hand-copy manual content into `wwwroot/`.
- No GDD viewer in-app — out of scope, confirmed with user.
- No new language picker — content language always follows `CultureInfo.CurrentUICulture.Name`, which the existing `LanguageSwitcher.razor` already sets (stores exactly `"en"` or `"pt-BR"` in localStorage, `Program.cs` applies it at boot).
- No bUnit — project convention is build + manual browser verification for UI; only the pure `ManualReference` mapping gets a unit test.
- resx parity: `AppStrings.resx` and `AppStrings.pt-BR.resx` must have the same `<data name=...>` count after every task that touches them (currently 992 each).
- Every new `Ruptura.Web/Services/*.cs` file matches the existing `I*ClientService` / `*ClientService` pattern (constructor-injected `IHttpClientFactory`, `factory.CreateClient("<name>")` — see `NotificationClientService.cs` for the exact shape to mirror).
- `_Imports.razor` already globally imports `Microsoft.AspNetCore.Components.Authorization`, `Microsoft.Extensions.Localization`, `Ruptura.Web.Resources`, `Ruptura.Web.Services`, `Ruptura.Web.Shared`, `Ruptura.Web.Pages` — do not add redundant `@using` lines for these in new `.razor` files.
- Never `git add -A` when committing a task — stage only the files that task touches, by exact path (two pre-existing unrelated dirty files, `.claude/settings.local.json` and `Makefile`, must never be swept into a commit).

---

### Task 1: `ManualReference` pure mapping helper

**Files:**
- Create: `src/Ruptura.Web/Services/ManualReference.cs`
- Test: `tests/Ruptura.UnitTests/Web/ManualReferenceTests.cs`

**Interfaces:**
- Produces: `enum ManualType { Player, GameMaster }` and `static class ManualReference { public static string FileNameFor(ManualType type, string culture) }`, both in namespace `Ruptura.Web.Services`. Every later task that needs to identify "which manual" uses `ManualType`; every later task that needs "which file to fetch" calls `ManualReference.FileNameFor`.

- [ ] **Step 1: Write the failing test**

Create `tests/Ruptura.UnitTests/Web/ManualReferenceTests.cs`:

```csharp
using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ManualReferenceTests
{
    [Theory]
    [InlineData(ManualType.Player, "pt-BR", "Manual_do_Jogador.md")]
    [InlineData(ManualType.Player, "en", "Manual_do_Jogador.en.md")]
    [InlineData(ManualType.GameMaster, "pt-BR", "Manual_do_Mestre.md")]
    [InlineData(ManualType.GameMaster, "en", "Manual_do_Mestre.en.md")]
    public void FileNameFor_MapsTypeAndCultureToFileName(ManualType type, string culture, string expected)
    {
        ManualReference.FileNameFor(type, culture).Should().Be(expected);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ruptura.UnitTests --filter ManualReferenceTests`
Expected: FAIL to build — `ManualType`/`ManualReference` do not exist yet.

- [ ] **Step 3: Write the implementation**

Create `src/Ruptura.Web/Services/ManualReference.cs`:

```csharp
namespace Ruptura.Web.Services;

public enum ManualType
{
    Player,
    GameMaster
}

/// <summary>
/// Pure mapping from (manual, language) to the Markdown file name served under
/// wwwroot/content/manuals/ — see docs/superpowers/specs/2026-08-14-manuals-page-design.md.
/// </summary>
public static class ManualReference
{
    public static string FileNameFor(ManualType type, string culture)
    {
        var baseName = type switch
        {
            ManualType.Player => "Manual_do_Jogador",
            ManualType.GameMaster => "Manual_do_Mestre",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        // LanguageSwitcher only ever stores exactly "en" or "pt-BR" (Layout/LanguageSwitcher.razor) —
        // match that literally rather than parsing/normalizing a general BCP-47 tag.
        var suffix = string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase) ? ".en" : string.Empty;
        return $"{baseName}{suffix}.md";
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ruptura.UnitTests --filter ManualReferenceTests`
Expected: PASS, 4 of 4.

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Services/ManualReference.cs tests/Ruptura.UnitTests/Web/ManualReferenceTests.cs
git commit -m "feat: add ManualReference pure (type, culture) -> filename mapping

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01MzB1ocK1a9e1xuHcvy7Yvc"
```

---

### Task 2: `IManualClientService` + same-origin HTTP client wiring

**Files:**
- Create: `src/Ruptura.Web/Services/IManualClientService.cs`
- Create: `src/Ruptura.Web/Services/ManualClientService.cs`
- Modify: `src/Ruptura.Web/Program.cs`

**Interfaces:**
- Consumes: `ManualType`, `ManualReference.FileNameFor(ManualType, string)` from Task 1.
- Produces: `IManualClientService.GetManualAsync(ManualType type, CancellationToken ct = default) -> Task<string?>` (raw Markdown, or `null` on any fetch failure). Registered in DI as `AddScoped<IManualClientService, ManualClientService>()`. Task 4's `ManualViewer` component injects and calls this.

- [ ] **Step 1: Add the interface**

Create `src/Ruptura.Web/Services/IManualClientService.cs`:

```csharp
namespace Ruptura.Web.Services;

public interface IManualClientService
{
    Task<string?> GetManualAsync(ManualType type, CancellationToken ct = default);
}
```

- [ ] **Step 2: Add the implementation**

Create `src/Ruptura.Web/Services/ManualClientService.cs`:

```csharp
using System.Globalization;

namespace Ruptura.Web.Services;

public class ManualClientService(IHttpClientFactory factory) : IManualClientService
{
    private HttpClient Http => factory.CreateClient("RupturaSelf");

    public async Task<string?> GetManualAsync(ManualType type, CancellationToken ct = default)
    {
        var fileName = ManualReference.FileNameFor(type, CultureInfo.CurrentUICulture.Name);
        try
        {
            return await Http.GetStringAsync($"content/manuals/{fileName}", ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
```

- [ ] **Step 3: Register the same-origin HTTP client and the service in `Program.cs`**

In `src/Ruptura.Web/Program.cs`, find this block:

```csharp
// HTTP client with JWT handler
builder.Services.AddTransient<JwtAuthorizationHandler>();
builder.Services.AddHttpClient("RupturaApi", client =>
    client.BaseAddress = new Uri(appConfig.ApiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();
```

Add immediately after it:

```csharp

// Same-origin static content (manuals, etc.) — no auth handler, not the API.
builder.Services.AddHttpClient("RupturaSelf", client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
```

Find this line:

```csharp
builder.Services.AddScoped<INotificationClientService, NotificationClientService>();
```

Add immediately after it:

```csharp
builder.Services.AddScoped<IManualClientService, ManualClientService>();
```

- [ ] **Step 4: Verify it builds**

Run: `dotnet build src/Ruptura.Web`
Expected: `Build succeeded.`, 0 errors.

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Services/IManualClientService.cs src/Ruptura.Web/Services/ManualClientService.cs src/Ruptura.Web/Program.cs
git commit -m "feat: add IManualClientService over a same-origin HttpClient

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01MzB1ocK1a9e1xuHcvy7Yvc"
```

---

### Task 3: Content pipeline — Markdig package + manuals copied into `wwwroot`

**Files:**
- Modify: `src/Ruptura.Web/Ruptura.Web.csproj`
- Modify: `/home/pro/Ruptura/.gitignore`

**Interfaces:**
- Produces: at build/publish time, every file matching `docs/manuais/*.md` (whatever exists there — currently `Manual_do_Jogador.md` and `Manual_do_Mestre.md`; `*.en.md` siblings are picked up automatically the moment the parallel translation effort adds them, no plan change needed) is copied to `wwwroot/content/manuals/<same filename>` and registered as a static web asset, fetchable at the relative URL `content/manuals/<filename>` — this is the exact path `ManualClientService` (Task 2) requests. Also produces the `Markdig` package Task 4 imports.

This mechanism was verified empirically before writing this plan: a `Content Include="<path outside the project>" Link="wwwroot\content\manuals\..."` item both (a) copies the physical file to `bin/<config>/net8.0/wwwroot/content/manuals/` and (b) is registered in `Ruptura.Web.staticwebassets.runtime.json` / `staticwebassets.build.json` — so it is served correctly both by the WASM dev server and in the nginx-served production build.

- [ ] **Step 1: Add the Markdig package reference**

In `src/Ruptura.Web/Ruptura.Web.csproj`, find:

```xml
    <PackageReference Include="Microsoft.Extensions.Localization" Version="8.0.12" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.3" />
```

Replace with:

```xml
    <PackageReference Include="Markdig" Version="1.3.2" />
    <PackageReference Include="Microsoft.Extensions.Localization" Version="8.0.12" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.3" />
```

- [ ] **Step 2: Add the manuals content pipeline item group**

In the same file, find:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Ruptura.Shared\Ruptura.Shared.csproj" />
  </ItemGroup>

</Project>
```

Replace with:

```xml
  <ItemGroup>
    <ProjectReference Include="..\Ruptura.Shared\Ruptura.Shared.csproj" />
  </ItemGroup>

  <!-- docs/manuais/*.md (pt-BR originals + .en.md translations) is the single edited source of
       the in-app manuals (see docs/superpowers/specs/2026-08-14-manuals-page-design.md). This
       copies whatever exists there into wwwroot/content/manuals/ at build/publish time — no
       manual duplication, and new language files picked up automatically once translated. -->
  <ItemGroup>
    <Content Include="..\..\docs\manuais\*.md" Link="wwwroot\content\manuals\%(Filename)%(Extension)"
             CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Ignore the derived copies**

`/home/pro/Ruptura/.gitignore` already contains `bin/` and `obj/` at the top — those cover the
build output copies. No further gitignore change is needed since the `Link` target
(`wwwroot\content\manuals\...`) only ever materializes under `bin/`, never inside the actual
`src/Ruptura.Web/wwwroot/` source folder on disk. Skip this step (documented here so it isn't
mistaken for an oversight) — do not modify `.gitignore`.

- [ ] **Step 4: Restore packages and build**

Run: `dotnet restore src/Ruptura.Web && dotnet build src/Ruptura.Web --nologo`
Expected: `Build succeeded.`, 0 errors, Markdig resolves without a version conflict.

- [ ] **Step 5: Verify the manuals actually land in the build output**

Run: `find src/Ruptura.Web/bin/Debug/net8.0/wwwroot/content/manuals -type f`
Expected: lists `Manual_do_Jogador.md` and `Manual_do_Mestre.md` (plus any `.en.md` files that
exist in `docs/manuais/` at the time this task runs).

Run: `grep -o "content/manuals/[^\"]*" src/Ruptura.Web/bin/Debug/net8.0/Ruptura.Web.staticwebassets.runtime.json`
Expected: same file names appear — confirms they're registered as static web assets, not just
copied.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Ruptura.Web.csproj
git commit -m "build: pull docs/manuais/*.md into wwwroot via Content+Link, add Markdig

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01MzB1ocK1a9e1xuHcvy7Yvc"
```

---

### Task 4: `ManualViewer` shared component + `.manual-content` styling

**Files:**
- Create: `src/Ruptura.Web/Shared/ManualViewer.razor`
- Modify: `src/Ruptura.Web/wwwroot/css/app.css`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `IManualClientService` (Task 2), `ManualType` (Task 1), `Markdig` (Task 3), `LoadingIndicator` (existing, `Shared/LoadingIndicator.razor`, takes `Text` parameter).
- Produces: `<ManualViewer Type="ManualType.Player" />` — a self-contained component that fetches
  once on init, renders loading/error/content states. Task 5's pages use this directly.
- New resx keys (both files): `Manuals.LoadError`.

- [ ] **Step 1: Add resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the final line:

```xml
  <data name="Gm.Content.ObjectiveType.Eliminacao"><value>Elimination</value></data>
</root>
```

Replace with:

```xml
  <data name="Gm.Content.ObjectiveType.Eliminacao"><value>Elimination</value></data>

  <!-- Manuals -->
  <data name="Manuals.LoadError"><value>Could not load this manual. Please try again later.</value></data>
</root>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the final line:

```xml
  <data name="Gm.Content.ObjectiveType.Eliminacao"><value>Eliminação</value></data>
</root>
```

Replace with:

```xml
  <data name="Gm.Content.ObjectiveType.Eliminacao"><value>Eliminação</value></data>

  <!-- Manuals -->
  <data name="Manuals.LoadError"><value>Não foi possível carregar este manual. Tente novamente mais tarde.</value></data>
</root>
```

- [ ] **Step 2: Verify resx parity**

Run: `grep -c "<data name=" src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`
Expected: both files report `993`.

- [ ] **Step 3: Create the component**

Create `src/Ruptura.Web/Shared/ManualViewer.razor`:

```razor
@using Markdig
@inject IStringLocalizer<AppStrings> L
@inject IManualClientService ManualService

@if (_loading)
{
    <LoadingIndicator Text="@L["Common.Loading"]" />
}
else if (_html is null)
{
    <div class="alert-danger">@L["Manuals.LoadError"]</div>
}
else
{
    <div class="manual-content">@((MarkupString)_html)</div>
}

@code {
    [Parameter, EditorRequired] public ManualType Type { get; set; }

    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private bool _loading = true;
    private string? _html;

    protected override async Task OnInitializedAsync()
    {
        var raw = await ManualService.GetManualAsync(Type);
        _html = raw is null ? null : Markdown.ToHtml(raw, Pipeline);
        _loading = false;
    }
}
```

- [ ] **Step 4: Add `.manual-content` styling**

In `src/Ruptura.Web/wwwroot/css/app.css`, append at the end of the file:

```css

/* ── Manual content (rendered Markdown) ─────────────────────────────────────── */
.manual-content {
    max-width: 68rem;
    font-family: var(--font-body);
    font-size: var(--text-base);
    line-height: 1.65;
    color: var(--text);
}
.manual-content h1, .manual-content h2, .manual-content h3,
.manual-content h4, .manual-content h5, .manual-content h6 {
    font-family: var(--font-display);
    color: var(--text);
    margin: 1.75rem 0 0.75rem;
    line-height: 1.3;
}
.manual-content h1 { font-size: var(--text-2xl); }
.manual-content h2 { font-size: var(--text-xl); border-bottom: 1px solid var(--border); padding-bottom: .35rem; }
.manual-content h3 { font-size: var(--text-lg); }
.manual-content h4, .manual-content h5, .manual-content h6 { font-size: var(--text-base); }
.manual-content p { margin: 0 0 1rem; }
.manual-content ul, .manual-content ol { margin: 0 0 1rem; padding-left: 1.5rem; }
.manual-content li { margin-bottom: .35rem; }
.manual-content strong { color: var(--text); }
.manual-content a { color: var(--link); }
.manual-content a:hover { color: var(--link-hover); }
.manual-content blockquote {
    margin: 0 0 1rem; padding: .5rem 1rem;
    border-left: 3px solid var(--accent);
    background: var(--bg-surface);
    color: var(--text-muted);
}
.manual-content code {
    font-family: var(--font-mono); font-size: var(--text-sm);
    background: var(--bg-surface); padding: .1rem .3rem; border-radius: var(--radius);
}
.manual-content pre {
    background: var(--bg-surface); border: 1px solid var(--border); border-radius: var(--radius);
    padding: 1rem; overflow-x: auto; margin: 0 0 1rem;
}
.manual-content pre code { background: none; padding: 0; }
.manual-content hr { border: none; border-top: 1px solid var(--border); margin: 2rem 0; }
.manual-content table {
    width: 100%; border-collapse: collapse; margin: 0 0 1.5rem; font-size: var(--text-sm);
}
.manual-content th, .manual-content td {
    border: 1px solid var(--border); padding: .5rem .75rem; text-align: left; vertical-align: top;
}
.manual-content th { background: var(--bg-surface); font-family: var(--font-display); font-weight: 600; }
@media (max-width: 640px) {
    .manual-content table { display: block; overflow-x: auto; white-space: nowrap; }
}
```

- [ ] **Step 5: Build**

Run: `dotnet build src/Ruptura.Web --nologo`
Expected: `Build succeeded.`, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Shared/ManualViewer.razor src/Ruptura.Web/wwwroot/css/app.css \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add ManualViewer (fetch + Markdig render) and .manual-content styling

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01MzB1ocK1a9e1xuHcvy7Yvc"
```

---

### Task 5: `/manuals` page, role split, nav entry

**Files:**
- Create: `src/Ruptura.Web/Pages/Manuals.razor`
- Create: `src/Ruptura.Web/Pages/GmManuals.razor`
- Create: `src/Ruptura.Web/Pages/PlayerManual.razor`
- Modify: `src/Ruptura.Web/Layout/NavMenu.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ManualViewer` (Task 4), `ManualType` (Task 1).
- Produces: route `/manuals`, nav link, resx keys `Nav.Manuals`, `Manuals.Title`,
  `Manuals.Tab.GameMaster`, `Manuals.Tab.Player`. Nothing later depends on this task — it's the
  final assembly.

- [ ] **Step 1: Add resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find:

```xml
  <data name="Nav.Logout"><value>Sign Out</value></data>
  <data name="Nav.Dashboard"><value>Dashboard</value></data>
```

Replace with:

```xml
  <data name="Nav.Logout"><value>Sign Out</value></data>
  <data name="Nav.Dashboard"><value>Dashboard</value></data>
  <data name="Nav.Manuals"><value>Manuals</value></data>
```

Then find the block added in Task 4 (now the last entries before `</root>`):

```xml
  <!-- Manuals -->
  <data name="Manuals.LoadError"><value>Could not load this manual. Please try again later.</value></data>
</root>
```

Replace with:

```xml
  <!-- Manuals -->
  <data name="Manuals.Title"><value>Manuals</value></data>
  <data name="Manuals.Tab.GameMaster"><value>GM Manual</value></data>
  <data name="Manuals.Tab.Player"><value>Player Manual</value></data>
  <data name="Manuals.LoadError"><value>Could not load this manual. Please try again later.</value></data>
</root>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find:

```xml
  <data name="Nav.Logout"><value>Sair</value></data>
  <data name="Nav.Dashboard"><value>Painel</value></data>
```

Replace with:

```xml
  <data name="Nav.Logout"><value>Sair</value></data>
  <data name="Nav.Dashboard"><value>Painel</value></data>
  <data name="Nav.Manuals"><value>Manuais</value></data>
```

Then find the block added in Task 4:

```xml
  <!-- Manuals -->
  <data name="Manuals.LoadError"><value>Não foi possível carregar este manual. Tente novamente mais tarde.</value></data>
</root>
```

Replace with:

```xml
  <!-- Manuals -->
  <data name="Manuals.Title"><value>Manuais</value></data>
  <data name="Manuals.Tab.GameMaster"><value>Manual do Mestre</value></data>
  <data name="Manuals.Tab.Player"><value>Manual do Jogador</value></data>
  <data name="Manuals.LoadError"><value>Não foi possível carregar este manual. Tente novamente mais tarde.</value></data>
</root>
```

- [ ] **Step 2: Verify resx parity**

Run: `grep -c "<data name=" src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`
Expected: both files report `997`.

- [ ] **Step 3: Create the Player branch**

Create `src/Ruptura.Web/Pages/PlayerManual.razor`:

```razor
<ManualViewer Type="ManualType.Player" />
```

- [ ] **Step 4: Create the GM branch (tabs)**

Create `src/Ruptura.Web/Pages/GmManuals.razor`:

```razor
@inject IStringLocalizer<AppStrings> L

<ul class="nav nav-tabs">
    <li class="nav-item">
        <button class="nav-link @(_activeTab == ManualType.GameMaster ? "active" : "")"
                @onclick="() => _activeTab = ManualType.GameMaster">
            @L["Manuals.Tab.GameMaster"]
        </button>
    </li>
    <li class="nav-item">
        <button class="nav-link @(_activeTab == ManualType.Player ? "active" : "")"
                @onclick="() => _activeTab = ManualType.Player">
            @L["Manuals.Tab.Player"]
        </button>
    </li>
</ul>

<div style="padding:1.5rem 0">
    <div style="display:@(_activeTab == ManualType.GameMaster ? "block" : "none")">
        <ManualViewer Type="ManualType.GameMaster" />
    </div>
    <div style="display:@(_activeTab == ManualType.Player ? "block" : "none")">
        <ManualViewer Type="ManualType.Player" />
    </div>
</div>

@code {
    // Both ManualViewer instances mount once on page load and stay mounted — switching tabs only
    // toggles CSS display, so each manual is fetched exactly once per page visit, never re-fetched
    // when flipping back and forth between tabs.
    private ManualType _activeTab = ManualType.GameMaster;
}
```

- [ ] **Step 5: Create the page**

Create `src/Ruptura.Web/Pages/Manuals.razor`:

```razor
@page "/manuals"
@attribute [Authorize]
@inject IStringLocalizer<AppStrings> L

<PageTitle>@L["Manuals.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Manuals.Title"]</h1>
    </div>

    <AuthorizeView Roles="GameMaster">
        <Authorized><GmManuals /></Authorized>
        <NotAuthorized><PlayerManual /></NotAuthorized>
    </AuthorizeView>
</div>
```

- [ ] **Step 6: Add the nav link**

In `src/Ruptura.Web/Layout/NavMenu.razor`, find:

```razor
            <span class="nav-section-label">@L["Nav.Dashboard"]</span>
            <NavLink class="nav-link" href="/dashboard" Match="NavLinkMatch.All" title="@L["Nav.Dashboard"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Dashboard"])</span>
                <span class="nav-link-text">@L["Nav.Dashboard"]</span>
            </NavLink>
```

Replace with:

```razor
            <span class="nav-section-label">@L["Nav.Dashboard"]</span>
            <NavLink class="nav-link" href="/dashboard" Match="NavLinkMatch.All" title="@L["Nav.Dashboard"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Dashboard"])</span>
                <span class="nav-link-text">@L["Nav.Dashboard"]</span>
            </NavLink>
            <NavLink class="nav-link" href="/manuals" title="@L["Nav.Manuals"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Manuals"])</span>
                <span class="nav-link-text">@L["Nav.Manuals"]</span>
            </NavLink>
```

- [ ] **Step 7: Build**

Run: `dotnet build --nologo`
Expected: `Build succeeded.`, 0 errors, across the whole solution.

- [ ] **Step 8: Run the full unit test suite**

Run: `dotnet test tests/Ruptura.UnitTests --nologo`
Expected: all pass, including the 4 `ManualReferenceTests` from Task 1.

- [ ] **Step 9: Manual browser verification**

Run: `make up` (or `dotnet run --project src/Ruptura.API` + serve `src/Ruptura.Web` per the
project's local-dev docs) and in a browser:
1. Log in as a GameMaster account → `/manuals` shows two tabs, both render their manual's
   content with headings/tables styled, switching tabs doesn't re-fetch (no loading flash on
   the second visit to a tab).
2. Log in as a Player account → `/manuals` shows the Player Manual directly, no tabs.
3. Toggle the language switcher (EN/PT) → reload happens (existing behavior) → the manual
   content is now in the other language.
4. Confirm the "Manuals" nav link appears for both roles, placed right under "Dashboard".

- [ ] **Step 10: Commit**

```bash
git add src/Ruptura.Web/Pages/Manuals.razor src/Ruptura.Web/Pages/GmManuals.razor \
        src/Ruptura.Web/Pages/PlayerManual.razor src/Ruptura.Web/Layout/NavMenu.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add /manuals page — GM sees both manuals tabbed, Player sees Player manual

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01MzB1ocK1a9e1xuHcvy7Yvc"
```

---

## Plan self-review notes (for the executor's awareness, not a task)

- **Spec coverage:** all spec sections have a task — content pipeline (Task 3), fetching (Task
  2), pure mapping (Task 1), page/role-split/tabs (Task 5), rendering/styling (Task 4), nav/i18n
  (Task 5), testing (Tasks 1 + 5 Step 8-9). GDD explicitly excluded per spec — no task references
  it.
- **Deviation from spec, intentional:** the spec's "File Structure" sketch put the pure helper
  under a new `Ruptura.Web/Content/` folder. This plan puts it in `Ruptura.Web/Services/` instead,
  alongside `IManualClientService`/`ManualClientService` — matching the project's actual existing
  convention (flat `Services/` folder holds all non-page Web logic, pure and impure alike; see
  `ThemeService.cs`, `LightboxService.cs` living there already). No behavior difference.
- **Deviation from spec, empirically-driven:** the spec described the content pipeline in the
  abstract ("Content Include with Link"); this plan pins the exact working form (verified via a
  disposable build spike before writing this plan — see Task 3's description) rather than leaving
  it for the implementer to discover.
- **Type consistency:** `ManualType` (Task 1) is referenced identically in Task 2
  (`GetManualAsync(ManualType, CancellationToken)`), Task 4 (`ManualViewer.Type` parameter), and
  Task 5 (`GmManuals`/`PlayerManual` usage) — same two enum members throughout, no renaming.
- **No placeholders:** every step has literal, complete code or an exact runnable command with an
  expected result.
