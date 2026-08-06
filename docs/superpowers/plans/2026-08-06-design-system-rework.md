# Design System Rework — Foundation & Usability Toolkit — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rework `Ruptura.Web`'s color palette, typography, and responsive layout inside the existing "Arcane Ledger" identity, and add a small reusable usability toolkit (toasts, confirmation dialog, loading/skeleton states, table search, breadcrumbs, keyboard shortcuts) — demonstrated end-to-end on one real page (`GmCatalog`) as the reference implementation for the rest of the app.

**Architecture:** Pure CSS custom properties in `wwwroot/css/app.css` (no build pipeline, no new dependencies) plus small C# services + Razor components in `src/Ruptura.Web/Services` and a new `src/Ruptura.Web/Shared` folder, following the existing `ThemeService`/`ThemeSwitcher` pattern. All new services are plain C# (no `IJSRuntime` in the service layer itself) so they can be unit-tested directly from `Ruptura.UnitTests`.

**Tech Stack:** ASP.NET Core Blazor WebAssembly 8, plain CSS (custom properties, no preprocessor), vanilla JS interop (`wwwroot/js/ruptura.js`), xUnit + FluentAssertions (`Ruptura.UnitTests`).

## Global Constraints

- No new npm/JS build pipeline, no CSS framework swap, no Blazor component library (MudBlazor/Radzen/etc.) — stay inside the current stack (spec §2).
- Keep the "Arcane Ledger" identity: Cinzel/DM Sans/JetBrains Mono, sharp corners (`--radius: 0`), tone of a dark-fantasy institutional registry (spec §1).
- No font-size anywhere below `--text-2xs` (11px) (spec §4).
- All colors introduced meet WCAG AA (≥4.5:1 normal text, ≥3:1 large text/UI) against the surface they render on (spec §3).
- Every new user-facing string goes through `IStringLocalizer<AppStrings>` with a key in **both** `AppStrings.resx` (English) and `AppStrings.pt-BR.resx` (Portuguese) — no hardcoded text (spec §7).
- `ThemeService`/`ThemeSwitcher` keep Light/System/Dark with "System" as the default; only their visual treatment changes (spec §7).
- Touch targets ≥ 44×44px for interactive elements at ≤1023px width (spec §5).
- Three responsive tiers: desktop ≥1024px, tablet 600–1023px (icon rail sidebar), mobile <600px (overlay sidebar, card-table) (spec §5).
- This plan covers spec §3, §4, §5, §6, and §7 in full, plus a first, concrete instance of the §8 Fase 3 rollout pattern (`GmCatalog`) — the remaining pages in §8 Fase 3 are follow-up plans, not part of this one.

**Full design spec:** `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`

---

## File Structure

```
src/Ruptura.Web/
├── wwwroot/
│   ├── css/app.css                    Modify — tokens, typography, responsive, new component styles
│   └── js/ruptura.js                  Modify — global Escape handler, search-shortcut binder
├── _Imports.razor                     Modify — add @using Ruptura.Web.Shared
├── Layout/
│   ├── MainLayout.razor               Modify — mounts ToastContainer/ConfirmDialog, global Escape wiring
│   ├── NavMenu.razor                  Modify — monogram + title per link (tablet rail + tooltip)
│   ├── NavMenu.razor.css              Delete — dead template boilerplate, unused by current markup
│   ├── ThemeSwitcher.razor            Modify — icons instead of L/S/D letters, localized labels
│   ├── ToastContainer.razor           Create — renders ToastService.Messages, auto-dismiss
│   └── ConfirmDialog.razor            Create — renders ConfirmService.Current as a modal
├── Services/
│   ├── ToastService.cs                Create — toast queue + Show/Success/Error/Dismiss
│   └── ConfirmService.cs              Create — AskAsync(...)→Task<bool> confirmation flow
├── Shared/
│   ├── TableFilter.cs                 Create — pure client-side search predicate
│   ├── TableSearchBox.razor           Create — search input + Ctrl+K//  shortcut
│   ├── LoadingIndicator.razor         Create — replaces the repeated spinner+text markup
│   ├── SkeletonRows.razor             Create — shimmer table-row placeholders
│   ├── BreadcrumbItem.cs              Create — (Text, Href) record consumed by Breadcrumbs
│   └── Breadcrumbs.razor              Create — renders a List<BreadcrumbItem> trail
├── Pages/
│   └── GmCatalog.razor                Modify — reference implementation: search, confirm, toast, loading, mobile card-table
└── Resources/
    ├── AppStrings.resx                Modify — new English keys
    └── AppStrings.pt-BR.resx          Modify — new Portuguese keys

tests/Ruptura.UnitTests/
├── Ruptura.UnitTests.csproj           Modify — add ProjectReference to Ruptura.Web
└── Web/
    ├── ToastServiceTests.cs           Create
    ├── ConfirmServiceTests.cs         Create
    └── TableFilterTests.cs            Create
```

---

## Task 1: Foundation CSS — color tokens, typography scale, focus ring, touch targets, mobile card-table

**Files:**
- Modify: `src/Ruptura.Web/wwwroot/css/app.css` (full-file replacement)

**Interfaces:**
- Produces: CSS custom properties consumed by every later task — `--text-2xs`…`--text-3xl` (typography scale), `--primary`/`--primary-dark`/`--accent`/`--accent-on-dark`/`--danger`/`--danger-dark`/`--danger-bg`/`--danger-border`/`--success`/`--info` (semantic color tokens), `--sidebar-link`/`--sidebar-link-hover`/`--sidebar-label` (sidebar text tokens), `--nav-width-rail` (used by Task 2), CSS classes `.btn-danger`, `.loading-inline`, `.skeleton-bar`, `.breadcrumbs`/`.breadcrumb-item`/`.breadcrumb-sep`, `.table-search`/`.table-search-input`, `.toast-stack`/`.toast-item`/`.toast-close`, `.confirm-overlay`/`.confirm-box`/`.confirm-title`/`.confirm-message`/`.confirm-actions`, `.ledger-table.stack-mobile` (mobile card-table, keyed by `data-label` attributes the consuming page sets on each `<td>`).

This is a pure-CSS, no-logic task — there is no unit test to write first. Verification is `dotnet build` (catches any Razor/CSS reference errors) plus the full visual pass in Task 13. Every declaration below is a deliberate, interdependent change (color tokens, type scale, and breakpoints all reference each other), so it lands as one file replacement rather than dozens of micro-edits.

- [ ] **Step 1: Replace the entire contents of `app.css`**

```css
/* ═══════════════════════════════════════════════════════════════════════════
   RUPTURA — Arcane Ledger Design System
   Aesthetic: Official registry of a dark fantasy institution
   Display : Cinzel (Roman inscription authority)
   Body    : DM Sans (clean contrast to display)
   Data    : JetBrains Mono (invite codes, IDs)
   ═══════════════════════════════════════════════════════════════════════════ */

/* ── Tokens — Light (parchment) ────────────────────────────────────────────── */
:root {
    --font-display : 'Cinzel', Georgia, serif;
    --font-body    : 'DM Sans', system-ui, sans-serif;
    --font-mono    : 'JetBrains Mono', 'Fira Code', monospace;

    /* Type scale — 11px floor, 16px body. Never go below --text-2xs anywhere. */
    --text-2xs : 0.6875rem;  /* 11px — tags/labels only */
    --text-xs  : 0.75rem;    /* 12px — nav, badges, metadata */
    --text-sm  : 0.875rem;   /* 14px — secondary text, tables */
    --text-base: 1rem;       /* 16px — body copy */
    --text-lg  : 1.125rem;
    --text-xl  : 1.375rem;   /* section titles */
    --text-2xl : 1.75rem;    /* page titles */
    --text-3xl : 2.25rem;    /* stat numbers */

    --bg           : #EDE9E1;
    --bg-surface   : #F5F2EC;
    --bg-nav       : #15100B;
    --text         : #1A1511;
    --text-muted   : #57504A;
    --text-faint   : #847A70;
    --border       : #CEC8BE;
    --border-strong: #A09690;

    --primary      : #7A1B1B;
    --primary-dark : #5A1313;
    --accent       : #8A6A22;
    --accent-dim   : #6E551B;
    --accent-on-dark: #D4AF5A; /* fixed value — for decorative use on the always-dark sidebar/hero, not theme-dependent */

    --danger       : #B3261E;
    --danger-dark  : #8C1E17;
    --danger-bg    : rgba(179,38,30,0.08);
    --danger-border: rgba(179,38,30,0.25);
    --success      : #2E7D32;
    --info         : #1A5FA8;

    --badge-active-bg  : #e8f5e9; --badge-active-text : var(--success);
    --badge-used-bg    : #e3f0ff; --badge-used-text   : var(--info);
    --badge-expired-bg : #f0eeec; --badge-expired-text: var(--text-muted);

    --sidebar-link       : rgba(255,255,255,0.72);
    --sidebar-link-hover : rgba(255,255,255,0.92);
    --sidebar-label      : rgba(255,255,255,0.45);

    --nav-width      : 220px;
    --nav-width-rail : 56px;
    --topbar-h       : 52px;
    --radius         : 0px;
}

/* ── Tokens — Dark (void) ───────────────────────────────────────────────────── */
[data-theme="dark"] {
    --bg           : #100D0A;
    --bg-surface   : #1C1813;
    --text         : #F0EAE0;
    --text-muted   : #B8AFA5;
    --text-faint   : #7A7168;
    --border       : #302A24;
    --border-strong: #443C34;

    --primary      : #B23A2E;
    --primary-dark : #C94E40;
    --accent       : #D4AF5A;
    --accent-dim   : #A9863F;

    --danger       : #F2685C;
    --danger-dark  : #F5847A;
    --danger-bg    : rgba(242,104,92,0.12);
    --danger-border: rgba(242,104,92,0.3);
    --success      : #6FCF8A;
    --info         : #7AB4F0;

    --badge-active-bg  : #1a3320; --badge-active-text : var(--success);
    --badge-used-bg    : #162040; --badge-used-text   : var(--info);
    --badge-expired-bg : #1e1a16; --badge-expired-text: var(--text-muted);
}
@media (prefers-color-scheme: dark) {
    :root:not([data-theme]) {
        --bg           : #100D0A;
        --bg-surface   : #1C1813;
        --text         : #F0EAE0;
        --text-muted   : #B8AFA5;
        --text-faint   : #7A7168;
        --border       : #302A24;
        --border-strong: #443C34;

        --primary      : #B23A2E;
        --primary-dark : #C94E40;
        --accent       : #D4AF5A;
        --accent-dim   : #A9863F;

        --danger       : #F2685C;
        --danger-dark  : #F5847A;
        --danger-bg    : rgba(242,104,92,0.12);
        --danger-border: rgba(242,104,92,0.3);
        --success      : #6FCF8A;
        --info         : #7AB4F0;

        --badge-active-bg  : #1a3320; --badge-active-text : var(--success);
        --badge-used-bg    : #162040; --badge-used-text   : var(--info);
        --badge-expired-bg : #1e1a16; --badge-expired-text: var(--text-muted);
    }
}

/* ── Base ───────────────────────────────────────────────────────────────────── */
*, *::before, *::after { box-sizing: border-box; margin: 0; }

html { scroll-behavior: smooth; }

body {
    background: var(--bg);
    color: var(--text);
    font-family: var(--font-body);
    font-size: var(--text-base);
    line-height: 1.6;
    min-height: 100vh;
    padding-top: var(--topbar-h);
    transition: background 0.2s, color 0.2s;
    -webkit-font-smoothing: antialiased;
}

a { color: var(--primary); text-decoration: none; }
a:hover { color: var(--primary-dark); }

h1, h2, h3 { font-family: var(--font-display); font-weight: 400; color: var(--text); }

/* Visible keyboard focus ring everywhere — never rely on default browser outline alone,
   never suppress it either. */
:focus-visible {
    outline: 2px solid var(--accent);
    outline-offset: 2px;
}
.sidebar :focus-visible {
    outline-color: var(--accent-on-dark);
}

/* ── Bootstrap overrides ────────────────────────────────────────────────────── */
.btn {
    border-radius: var(--radius);
    font-family: var(--font-body);
    font-size: var(--text-xs);
    font-weight: 500;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    padding: 0.65rem 1.75rem;
    transition: background 0.15s, border-color 0.15s, color 0.15s;
}
.btn-primary {
    background: var(--primary) !important;
    border: 1px solid var(--primary) !important;
    color: #fff !important;
}
.btn-primary:hover, .btn-primary:focus {
    background: var(--primary-dark) !important;
    border-color: var(--primary-dark) !important;
}
.btn-primary:disabled { opacity: 0.5; }

.btn-danger {
    background: var(--danger) !important;
    border: 1px solid var(--danger) !important;
    color: #fff !important;
}
.btn-danger:hover, .btn-danger:focus {
    background: var(--danger-dark) !important;
    border-color: var(--danger-dark) !important;
}

.btn-outline-primary {
    background: transparent !important;
    border: 1px solid var(--primary) !important;
    color: var(--primary) !important;
}
.btn-outline-primary:hover {
    background: var(--primary) !important;
    color: #fff !important;
}
.btn-outline-secondary {
    background: transparent !important;
    border: 1px solid var(--border) !important;
    color: var(--text-muted) !important;
    padding: 0.4rem 0.9rem;
}
.btn-outline-secondary:hover {
    border-color: var(--border-strong) !important;
    color: var(--text) !important;
}
.btn-sm { padding: 0.4rem 1rem; font-size: var(--text-2xs); }

/* Form elements — ledger style */
.form-label {
    display: block;
    font-size: var(--text-2xs);
    font-weight: 500;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--text-muted);
    margin-bottom: 0.4rem;
}
.form-control, .form-select {
    background: transparent !important;
    border: none !important;
    border-bottom: 1px solid var(--border) !important;
    border-radius: 0 !important;
    color: var(--text) !important;
    font-family: var(--font-body) !important;
    font-size: var(--text-base) !important;
    padding: 0.45rem 0 !important;
    box-shadow: none !important;
    transition: border-color 0.15s !important;
}
.form-control:focus, .form-select:focus {
    border-bottom-color: var(--primary) !important;
    box-shadow: none !important;
}
.form-control::placeholder { color: var(--text-faint) !important; }

/* Type-ahead autocomplete dropdown (single searchable input, no external component) */
.autocomplete-list {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    margin-top: 2px;
    background: var(--bg-surface);
    border: 1px solid var(--border);
    max-height: 220px;
    overflow-y: auto;
    z-index: 50;
}
.autocomplete-item {
    padding: 0.5rem 0.75rem;
    font-size: var(--text-sm);
    color: var(--text);
    cursor: pointer;
}
.autocomplete-item:hover {
    background: var(--bg);
}

.validation-message { color: var(--danger); font-size: var(--text-xs); margin-top: 0.3rem; }
.valid.modified:not([type=checkbox]) { border-bottom-color: var(--success) !important; }
.invalid { border-bottom-color: var(--danger) !important; }

.alert-danger {
    background: var(--danger-bg);
    border: 1px solid var(--danger-border);
    color: var(--danger);
    border-radius: 0;
    font-size: var(--text-sm);
    padding: 0.75rem 1rem;
}

/* ── App Shell ─────────────────────────────────────────────────────────────── */
.app-shell {
    /* bare wrapper — body handles min-height, sidebar+topbar are fixed */
}

/* ── Sidebar ────────────────────────────────────────────────────────────────── */
.sidebar {
    width: var(--nav-width);
    background: var(--bg-nav);
    display: flex;
    flex-direction: column;
    position: fixed;
    inset: 0 auto 0 0;
    z-index: 200;
    transition: transform 0.25s ease, width 0.2s ease;
    border-right: 1px solid rgba(255,255,255,0.04);
}

.sidebar-brand {
    display: block;
    padding: 1.75rem 1.5rem 1.25rem;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    text-decoration: none;
}
.brand-name {
    font-family: var(--font-display);
    font-size: var(--text-sm);
    font-weight: 700;
    letter-spacing: 0.35em;
    color: #fff;
    display: block;
}
.brand-tagline {
    font-size: var(--text-2xs);
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: rgba(255,255,255,0.4);
    margin-top: 0.3rem;
    display: block;
}

.sidebar-nav {
    padding: 1.25rem 0;
    flex: 1;
    overflow-y: auto;
}
.nav-section-label {
    font-size: var(--text-2xs);
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--sidebar-label);
    padding: 0.5rem 1.5rem 0.25rem;
    display: block;
}
.sidebar-nav a.nav-link {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--sidebar-link);
    padding: 0.65rem 1.5rem;
    border-left: 2px solid transparent;
    transition: color 0.15s, border-color 0.15s, background 0.15s;
    text-decoration: none;
}
.sidebar-nav a.nav-link:hover {
    color: var(--sidebar-link-hover);
    background: rgba(255,255,255,0.05);
}
.sidebar-nav a.nav-link.active {
    color: #fff;
    border-left-color: var(--accent-on-dark);
    background: rgba(255,255,255,0.06);
}
.nav-btn-link {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    width: 100%;
    text-align: left;
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--sidebar-link);
    padding: 0.65rem 1.5rem;
    background: none;
    border: none;
    border-left: 2px solid transparent;
    cursor: pointer;
    transition: color 0.15s;
}
.nav-btn-link:hover { color: var(--sidebar-link-hover); }

.sidebar-footer {
    padding: 1rem 1.5rem;
    border-top: 1px solid rgba(255,255,255,0.06);
}

/* ── Main Wrapper ───────────────────────────────────────────────────────────── */
.main-wrapper {
    margin-left: var(--nav-width);
    min-height: calc(100vh - var(--topbar-h));
    transition: margin-left 0.2s ease;
}

/* ── Top Bar ────────────────────────────────────────────────────────────────── */
.top-bar {
    position: fixed;
    top: 0;
    left: var(--nav-width);
    right: 0;
    height: var(--topbar-h);
    background: var(--bg);
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    padding: 0 1.75rem;
    gap: 1rem;
    z-index: 100;
    transition: left 0.2s ease;
}
.top-bar-title {
    font-size: var(--text-2xs);
    font-weight: 500;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--text-muted);
    flex: 1;
}
.top-bar-controls {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

/* Mobile hamburger */
.hamburger {
    display: none;
    flex-direction: column;
    gap: 4px;
    cursor: pointer;
    background: none;
    border: none;
    padding: 4px;
}
.hamburger span {
    display: block;
    width: 18px;
    height: 1px;
    background: var(--text-muted);
    transition: 0.2s;
}

.page-content {
    padding: 2rem 2.25rem;
    max-width: 960px;
    margin: 0 auto;
}

/* ── Page Headings ──────────────────────────────────────────────────────────── */
.page-heading {
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid var(--border);
}
.page-heading h1 {
    font-family: var(--font-display);
    font-size: clamp(1.1rem, 1rem + 1vw, var(--text-2xl));
    font-weight: 400;
    letter-spacing: 0.1em;
    color: var(--text);
}
.page-heading p {
    font-size: var(--text-sm);
    color: var(--text-muted);
    margin-top: 0.25rem;
}

/* ── Section header ─────────────────────────────────────────────────────────── */
.section-header {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    margin-bottom: 1.25rem;
    flex-wrap: wrap;
    gap: 0.75rem;
}
.section-title {
    font-size: var(--text-2xs);
    font-weight: 500;
    letter-spacing: 0.15em;
    text-transform: uppercase;
    color: var(--text-muted);
}

/* ── Ledger Stats ───────────────────────────────────────────────────────────── */
.ledger-stats {
    display: flex;
    gap: 2.5rem;
    padding-bottom: 2rem;
    border-bottom: 1px solid var(--border);
    margin-bottom: 2rem;
    flex-wrap: wrap;
}
.stat { min-width: 60px; }
.stat-num {
    font-family: var(--font-display);
    font-size: var(--text-3xl);
    font-weight: 400;
    line-height: 1;
    color: var(--text);
}
.stat-label {
    font-size: var(--text-2xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--text-faint);
    margin-top: 0.3rem;
}
.stat.s-active .stat-num  { color: var(--success); }
.stat.s-used   .stat-num  { color: var(--accent); }
.stat.s-expired .stat-num { color: var(--text-muted); }

/* ── New code banner ────────────────────────────────────────────────────────── */
.new-code-banner {
    display: flex;
    align-items: center;
    gap: 2rem;
    padding: 1.25rem 0;
    border-top: 1px solid var(--accent);
    border-bottom: 1px solid var(--border);
    margin-bottom: 2rem;
    flex-wrap: wrap;
}
.banner-label {
    font-size: var(--text-2xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--text-muted);
    margin-bottom: 0.2rem;
}
.banner-code {
    font-family: var(--font-mono);
    font-size: var(--text-xl);
    letter-spacing: 0.2em;
    color: var(--accent);
}
.banner-note {
    font-size: var(--text-xs);
    color: var(--text-faint);
    margin-left: auto;
}

/* ── Ledger Table ───────────────────────────────────────────────────────────── */
.ledger-table-wrap { overflow-x: auto; }
.ledger-table {
    width: 100%;
    border-collapse: collapse;
}
.ledger-table th {
    font-size: var(--text-2xs);
    font-weight: 500;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--text-faint);
    text-align: left;
    padding: 0 0.5rem 0.65rem 0;
    border-bottom: 1px solid var(--border);
    white-space: nowrap;
}
.ledger-table td {
    padding: 0.8rem 0.5rem 0.8rem 0;
    border-bottom: 1px solid var(--border);
    font-size: var(--text-sm);
    color: var(--text);
    vertical-align: middle;
}
.ledger-table tr:last-child td { border-bottom: none; }
.ledger-table tr:hover td { background: rgba(0,0,0,0.02); }

/* Code chip */
.code-chip {
    font-family: var(--font-mono);
    font-size: var(--text-sm);
    color: var(--text);
    cursor: pointer;
    border-bottom: 1px dashed var(--border);
    padding-bottom: 1px;
    transition: color 0.15s, border-color 0.15s;
    letter-spacing: 0.05em;
}
.code-chip:hover { color: var(--accent); border-bottom-color: var(--accent); }

/* Status badges */
.badge-status {
    display: inline-block;
    font-size: var(--text-2xs);
    letter-spacing: 0.08em;
    text-transform: uppercase;
    padding: 0.2rem 0.55rem;
    border-radius: 0;
}
.badge-active  { background: var(--badge-active-bg);  color: var(--badge-active-text); }
.badge-used    { background: var(--badge-used-bg);    color: var(--badge-used-text); }
.badge-expired { background: var(--badge-expired-bg); color: var(--badge-expired-text); }

/* Copy button */
.copy-btn {
    background: none;
    border: none;
    color: var(--text-faint);
    cursor: pointer;
    font-size: var(--text-sm);
    padding: 0.2rem 0.4rem;
    transition: color 0.15s;
}
.copy-btn:hover { color: var(--accent); }

/* Empty state */
.ledger-empty {
    padding: 3rem 0;
    text-align: center;
    border-top: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
    color: var(--text-muted);
    font-size: var(--text-sm);
}
.ledger-empty p { margin-bottom: 1.25rem; }

/* Inline loading indicator (LoadingIndicator.razor) */
.loading-inline {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    padding: 2rem 0;
    color: var(--text-muted);
    font-size: var(--text-sm);
}

/* Skeleton rows (SkeletonRows.razor) */
.skeleton-bar {
    display: block;
    height: 0.85rem;
    width: 100%;
    border-radius: 2px;
    background: linear-gradient(90deg, var(--border) 25%, var(--border-strong) 37%, var(--border) 63%);
    background-size: 400% 100%;
    animation: skeleton-shimmer 1.4s ease infinite;
}
@keyframes skeleton-shimmer {
    0%   { background-position: 100% 50%; }
    100% { background-position: 0 50%; }
}
@media (prefers-reduced-motion: reduce) {
    .skeleton-bar { animation: none; }
}

/* Breadcrumbs (Breadcrumbs.razor) */
.breadcrumbs {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.4rem;
    font-size: var(--text-xs);
    letter-spacing: 0.04em;
    color: var(--text-muted);
    margin-bottom: 1.25rem;
}
.breadcrumbs a { color: var(--text-muted); }
.breadcrumbs a:hover { color: var(--text); }
.breadcrumb-sep { color: var(--text-faint); }
.breadcrumb-item [aria-current="page"] { color: var(--text); }

/* Table search box (TableSearchBox.razor) */
.table-search { margin-bottom: 1rem; max-width: 320px; }
.table-search-input { font-size: var(--text-sm) !important; }

/* Toasts (ToastContainer.razor) */
.toast-stack {
    position: fixed;
    top: calc(var(--topbar-h) + 1rem);
    right: 1.25rem;
    z-index: 500;
    display: flex;
    flex-direction: column;
    gap: 0.6rem;
    max-width: 360px;
}
.toast-item {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
    padding: 0.85rem 1rem;
    background: var(--bg-surface);
    border: 1px solid var(--border-strong);
    border-left: 3px solid var(--text-muted);
    box-shadow: 0 4px 16px rgba(0,0,0,0.18);
    font-size: var(--text-sm);
    color: var(--text);
    animation: toast-in 0.2s ease both;
}
.toast-item.toast-success { border-left-color: var(--success); }
.toast-item.toast-error   { border-left-color: var(--danger); }
.toast-item.toast-info    { border-left-color: var(--info); }
.toast-text { flex: 1; }
.toast-close {
    background: none;
    border: none;
    color: var(--text-faint);
    cursor: pointer;
    font-size: var(--text-base);
    line-height: 1;
    padding: 0;
}
.toast-close:hover { color: var(--text); }
@keyframes toast-in {
    from { opacity: 0; transform: translateY(-6px); }
    to   { opacity: 1; transform: translateY(0); }
}
@media (prefers-reduced-motion: reduce) {
    .toast-item { animation: none; }
}

/* Confirm dialog (ConfirmDialog.razor) */
.confirm-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0,0,0,0.55);
    z-index: 600;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1.5rem;
}
.confirm-box {
    background: var(--bg-surface);
    border: 1px solid var(--border-strong);
    max-width: 400px;
    width: 100%;
    padding: 1.75rem;
}
.confirm-title {
    font-family: var(--font-display);
    font-size: var(--text-lg);
    font-weight: 400;
    color: var(--text);
    margin-bottom: 0.75rem;
}
.confirm-message {
    font-size: var(--text-sm);
    color: var(--text-muted);
    line-height: 1.6;
    margin-bottom: 1.5rem;
}
.confirm-actions {
    display: flex;
    justify-content: flex-end;
    gap: 0.6rem;
}

/* ── Theme Switcher ─────────────────────────────────────────────────────────── */
.theme-switcher {
    display: flex;
    gap: 0;
    border: 1px solid var(--border);
}
.theme-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    background: none;
    border: none;
    color: var(--text-faint);
    cursor: pointer;
    padding: 0.4rem 0.55rem;
    transition: color 0.15s, background 0.15s;
}
.theme-btn:hover { color: var(--text); }
.theme-btn.active {
    background: var(--text);
    color: var(--bg);
}
.theme-btn svg { width: 14px; height: 14px; }

/* ── Language Switcher ──────────────────────────────────────────────────────── */
.language-switcher {
    display: flex;
    gap: 0;
    border: 1px solid rgba(255,255,255,0.15);
}
.lang-btn {
    background: none;
    border: none;
    color: rgba(255,255,255,0.55);
    cursor: pointer;
    font-family: var(--font-body);
    font-size: var(--text-2xs);
    font-weight: 500;
    letter-spacing: 0.1em;
    padding: 0.3rem 0.55rem;
    transition: color 0.15s, background 0.15s;
    text-transform: uppercase;
}
.lang-btn:hover { color: rgba(255,255,255,0.85); }
.lang-btn.active {
    background: rgba(255,255,255,0.15);
    color: #fff;
}

/* User chip */
.user-chip {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    font-size: var(--text-sm);
}
.user-chip-name { color: var(--text-muted); font-size: var(--text-sm); }
.user-role-badge {
    font-size: var(--text-2xs);
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--accent);
    border: 1px solid var(--accent);
    padding: 0.1rem 0.45rem;
}

/* ── Auth Pages (no card — ledger form on bare background) ──────────────────── */
.auth-scene {
    min-height: calc(100vh - var(--topbar-h));
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 2rem;
}
.auth-form-wrap {
    width: 100%;
    max-width: 380px;
}
.auth-logo {
    text-align: center;
    margin-bottom: 2.5rem;
}
.auth-logo-name {
    font-family: var(--font-display);
    font-size: var(--text-base);
    font-weight: 700;
    letter-spacing: 0.4em;
    color: var(--text);
    display: block;
}
.auth-logo-rule {
    display: block;
    height: 1px;
    background: var(--border);
    margin: 0.6rem auto;
    width: 40px;
}
.auth-logo-sub {
    font-size: var(--text-2xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: var(--text-muted);
}
.auth-field {
    margin-bottom: 1.75rem;
}
.auth-actions {
    margin-top: 2.25rem;
}
.auth-actions .btn {
    width: 100%;
}
.auth-footer {
    text-align: center;
    margin-top: 1.75rem;
    font-size: var(--text-sm);
    color: var(--text-faint);
}
.auth-footer a {
    color: var(--text-muted);
    border-bottom: 1px solid var(--border);
    padding-bottom: 1px;
    transition: color 0.15s, border-color 0.15s;
}
.auth-footer a:hover { color: var(--text); border-bottom-color: var(--border-strong); }

.auth-toggle {
    display: flex;
    gap: 0;
    border-bottom: 1px solid var(--border);
    margin-bottom: 2rem;
}
.auth-toggle-btn {
    background: none;
    border: none;
    border-bottom: 2px solid transparent;
    margin-bottom: -1px;
    padding: 0.5rem 0;
    margin-right: 2rem;
    font-family: var(--font-body);
    font-size: var(--text-xs);
    font-weight: 500;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--text-muted);
    cursor: pointer;
    transition: color 0.15s, border-color 0.15s;
}
.auth-toggle-btn.active {
    color: var(--text);
    border-bottom-color: var(--primary);
}

.password-hint {
    font-size: var(--text-xs);
    color: var(--text-faint);
    text-align: center;
    margin-top: 1.25rem;
    letter-spacing: 0.03em;
}

/* ── Landing Page ───────────────────────────────────────────────────────────── */
.landing { min-height: 100vh; }

/* Hero */
.landing-hero {
    min-height: calc(100vh - var(--topbar-h));
    display: flex;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: 4rem 2rem;
    background: var(--bg-nav);
    position: relative;
}
.hero-inner {
    max-width: 640px;
}
.hero-rule {
    display: block;
    height: 1px;
    background: rgba(255,255,255,0.12);
    margin: 0 auto 1.5rem;
    width: 80px;
    transition: width 0.6s ease;
}
.hero-inner:hover .hero-rule { width: 120px; }
.hero-title {
    font-family: var(--font-display);
    font-size: clamp(2.5rem, 10vw, 6rem);
    font-weight: 700;
    letter-spacing: 0.3em;
    color: #fff;
    line-height: 1;
    margin-bottom: 1.5rem;
}
.hero-subtitle {
    font-size: var(--text-2xs);
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--accent-on-dark);
    margin-bottom: 2rem;
    font-family: var(--font-body);
}
.hero-desc {
    font-size: var(--text-base);
    line-height: 1.8;
    color: rgba(255,255,255,0.6);
    max-width: 480px;
    margin: 0 auto 3rem;
}
.hero-links {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0;
    flex-wrap: wrap;
}
.hero-link {
    font-size: var(--text-xs);
    letter-spacing: 0.12em;
    text-transform: uppercase;
    color: rgba(255,255,255,0.6);
    text-decoration: none;
    padding: 0.75rem 1.5rem;
    border: 1px solid rgba(255,255,255,0.15);
    transition: color 0.15s, border-color 0.15s, background 0.15s;
}
.hero-link:hover {
    color: #fff;
    border-color: rgba(255,255,255,0.35);
}
.hero-link.primary {
    background: var(--primary);
    border-color: var(--primary);
    color: #fff;
}
.hero-link.primary:hover {
    background: var(--primary-dark);
    border-color: var(--primary-dark);
}

@keyframes fadeUp {
    from { opacity: 0; transform: translateY(12px); }
    to   { opacity: 1; transform: translateY(0); }
}
@media (prefers-reduced-motion: no-preference) {
    .hero-inner { animation: fadeUp 0.7s ease both; }
}

/* Pillars */
.landing-pillars {
    padding: 4rem 2rem;
    background: var(--bg);
    border-top: 1px solid var(--border);
}
.pillars-header {
    text-align: center;
    margin-bottom: 3rem;
}
.pillars-eyebrow {
    font-size: var(--text-2xs);
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--text-faint);
    display: block;
    margin-bottom: 0.5rem;
}
.pillars-heading {
    font-family: var(--font-display);
    font-size: var(--text-xl);
    font-weight: 400;
    letter-spacing: 0.08em;
    color: var(--text);
}
.pillars-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: 0;
    max-width: 900px;
    margin: 0 auto;
    border-top: 1px solid var(--border);
    border-left: 1px solid var(--border);
}
.pillar-item {
    padding: 1.75rem;
    border-right: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
}
.pillar-title {
    font-size: var(--text-sm);
    font-weight: 500;
    color: var(--text);
    margin-bottom: 0.5rem;
    letter-spacing: 0.02em;
}
.pillar-desc {
    font-size: var(--text-sm);
    color: var(--text-muted);
    line-height: 1.65;
}

/* Quote */
.landing-quote {
    padding: 3rem 2rem;
    text-align: center;
    background: var(--bg-surface);
    border-top: 1px solid var(--border);
    border-bottom: 1px solid var(--border);
}
.landing-quote blockquote {
    font-family: var(--font-display);
    font-size: clamp(0.95rem, 2vw, 1.1rem);
    font-weight: 400;
    font-style: italic;
    color: var(--text-muted);
    max-width: 680px;
    margin: 0 auto;
    line-height: 1.8;
}
.landing-quote blockquote::before { content: '«\00a0'; color: var(--accent); }
.landing-quote blockquote::after  { content: '\00a0»'; color: var(--accent); }

/* CTA */
.landing-cta {
    padding: 4rem 2rem;
    text-align: center;
    background: var(--bg);
}
.landing-cta h2 {
    font-family: var(--font-display);
    font-size: var(--text-xl);
    font-weight: 400;
    letter-spacing: 0.08em;
    color: var(--text);
    margin-bottom: 0.5rem;
}
.landing-cta p {
    font-size: var(--text-sm);
    color: var(--text-muted);
    margin-bottom: 1.75rem;
}

/* ── Blazor loading ─────────────────────────────────────────────────────────── */
.blazor-loading {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: var(--bg-nav, #15100B);
}
.loading-progress {
    display: block;
    width: 5rem;
    height: 5rem;
    margin-bottom: 1.5rem;
}
.loading-progress circle {
    fill: none;
    stroke: rgba(255,255,255,0.1);
    stroke-width: 0.5rem;
    transform-origin: 50% 50%;
    transform: rotate(-90deg);
}
.loading-progress circle:last-child {
    stroke: var(--primary, #7A1B1B);
    stroke-dasharray: calc(3.141 * var(--blazor-load-percentage, 0%) * 0.8), 500%;
    transition: stroke-dasharray 0.05s ease-in-out;
}
.loading-label {
    font-family: 'Cinzel', serif;
    font-size: var(--text-xs);
    letter-spacing: 0.4em;
    color: rgba(255,255,255,0.3);
    text-transform: uppercase;
}

/* Blazor error UI */
#blazor-error-ui {
    background: var(--danger);
    bottom: 0; left: 0;
    display: none;
    padding: 0.75rem 1.5rem;
    position: fixed;
    width: 100%;
    z-index: 9999;
    color: #fff;
    font-size: var(--text-sm);
    letter-spacing: 0.05em;
}
#blazor-error-ui .reload { color: #fff; text-decoration: underline; margin-left: 1rem; }
#blazor-error-ui .dismiss { float: right; cursor: pointer; }
.blazor-error-boundary { background: var(--danger); padding: 1rem; color: #fff; }
.blazor-error-boundary::after { content: "An error has occurred."; }

/* ── Responsive ─────────────────────────────────────────────────────────────── */

/* Touch targets — anything interactive gets a comfortable minimum hit area
   from tablet width down, where pointer precision (mouse) can no longer be assumed. */
@media (max-width: 1023px) {
    .btn, .hamburger, .copy-btn, .theme-btn, .lang-btn,
    .sidebar-nav a.nav-link, .nav-btn-link, .toast-close {
        min-height: 44px;
    }
    .btn, .sidebar-nav a.nav-link, .nav-btn-link {
        min-width: 44px;
    }
}

/* Tablet: handled by Task 2 (sidebar rail — depends on NavMenu markup) */

/* Mobile */
@media (max-width: 599px) {
    .sidebar { transform: translateX(-100%); width: var(--nav-width); }
    .sidebar.open { transform: translateX(0); }
    .top-bar { left: 0; }
    .main-wrapper { margin-left: 0; min-height: calc(100vh - var(--topbar-h)); }
    .hamburger { display: flex; }
    .page-content { padding: 1.25rem 1rem; }
    .hero-title { letter-spacing: 0.15em; }
    .ledger-stats { gap: 1.5rem; }
    .pillars-grid { grid-template-columns: 1fr; }

    /* Ledger tables become stacked cards — one per row, labelled by the
       column header via a data attribute the page sets on each <td>. */
    .ledger-table.stack-mobile,
    .ledger-table.stack-mobile thead,
    .ledger-table.stack-mobile tbody,
    .ledger-table.stack-mobile th,
    .ledger-table.stack-mobile td,
    .ledger-table.stack-mobile tr {
        display: block;
    }
    .ledger-table.stack-mobile thead { position: absolute; left: -9999px; }
    .ledger-table.stack-mobile tr {
        border: 1px solid var(--border);
        margin-bottom: 0.85rem;
        padding: 0.5rem 0.75rem;
    }
    .ledger-table.stack-mobile td {
        border-bottom: none;
        padding: 0.4rem 0;
        display: flex;
        justify-content: space-between;
        gap: 1rem;
    }
    .ledger-table.stack-mobile td::before {
        content: attr(data-label);
        font-size: var(--text-2xs);
        letter-spacing: 0.1em;
        text-transform: uppercase;
        color: var(--text-faint);
        flex-shrink: 0;
    }
}
```

- [ ] **Step 2: Build to verify no compile errors ripple from the CSS change**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.` (Blazor WASM doesn't compile CSS, but this catches any accidental breakage in the project).

- [ ] **Step 3: Commit**

```bash
git add src/Ruptura.Web/wwwroot/css/app.css
git commit -m "feat: rework color tokens, typography scale, and responsive foundation"
```

---

## Task 2: Sidebar tablet rail — NavMenu markup + rail CSS + remove dead scoped CSS

**Files:**
- Modify: `src/Ruptura.Web/Layout/NavMenu.razor`
- Modify: `src/Ruptura.Web/wwwroot/css/app.css` (append rail rules)
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`
- Delete: `src/Ruptura.Web/Layout/NavMenu.razor.css`

**Interfaces:**
- Consumes: `--nav-width-rail`, `--sidebar-link`, `--accent-on-dark` (Task 1).
- Produces: `.nav-link-mono` CSS class and the monogram+title markup pattern later pages can copy for any new sidebar entries.

`NavMenu.razor.css` is leftover Blazor project-template boilerplate (`.bi-house-door-fill-nav-menu` etc.) — none of its selectors (`.nav-item`, `.bi`) appear anywhere in the current `NavMenu.razor` markup. It's dead weight that would confuse anyone looking for where the sidebar is actually styled (that's `app.css`). Removing it while touching this component is a direct, in-scope cleanup.

- [ ] **Step 1: Replace `NavMenu.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject IAuthClientService AuthService
@inject NavigationManager Nav

<nav class="sidebar-nav">
    <AuthorizeView>
        <Authorized>
            <span class="nav-section-label">@L["Nav.Dashboard"]</span>
            <NavLink class="nav-link" href="/dashboard" Match="NavLinkMatch.All" title="@L["Nav.Dashboard"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Dashboard"])</span>
                <span class="nav-link-text">@L["Nav.Dashboard"]</span>
            </NavLink>

            <AuthorizeView Roles="Player">
                <Authorized Context="playerCtx">
                    <NavLink class="nav-link" href="/campaigns" title="@L["Nav.Campaigns.Player"]">
                        <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Campaigns.Player"])</span>
                        <span class="nav-link-text">@L["Nav.Campaigns.Player"]</span>
                    </NavLink>
                </Authorized>
            </AuthorizeView>

            <AuthorizeView Roles="GameMaster">
                <Authorized Context="gmCtx">
                    <span class="nav-section-label" style="margin-top:.75rem">@L["Nav.Section.GameMaster"]</span>
                    <NavLink class="nav-link" href="/gm/players" title="@L["Nav.Players"]">
                        <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Players"])</span>
                        <span class="nav-link-text">@L["Nav.Players"]</span>
                    </NavLink>
                    <NavLink class="nav-link" href="/gm/campaigns" title="@L["Nav.Campaigns"]">
                        <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Campaigns"])</span>
                        <span class="nav-link-text">@L["Nav.Campaigns"]</span>
                    </NavLink>
                    <NavLink class="nav-link" href="/gm/notifications" title="@L["Nav.Notifications"]">
                        <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Notifications"])</span>
                        <span class="nav-link-text">@L["Nav.Notifications"]</span>
                    </NavLink>
                </Authorized>
            </AuthorizeView>

            <div style="flex:1"></div>
            <button class="nav-btn-link" @onclick="LogoutAsync" title="@L["Nav.Logout"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Logout"])</span>
                <span class="nav-link-text">@L["Nav.Logout"]</span>
            </button>
        </Authorized>
        <NotAuthorized>
            <NavLink class="nav-link" href="/" Match="NavLinkMatch.All" title="@L["Nav.Home"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Home"])</span>
                <span class="nav-link-text">@L["Nav.Home"]</span>
            </NavLink>
            <NavLink class="nav-link" href="/login" title="@L["Nav.Login"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Login"])</span>
                <span class="nav-link-text">@L["Nav.Login"]</span>
            </NavLink>
            <NavLink class="nav-link" href="/register" title="@L["Nav.Register"]">
                <span class="nav-link-mono" aria-hidden="true">@Initial(L["Nav.Register"])</span>
                <span class="nav-link-text">@L["Nav.Register"]</span>
            </NavLink>
        </NotAuthorized>
    </AuthorizeView>
</nav>

@code {
    private async Task LogoutAsync()
    {
        await AuthService.LogoutAsync();
        Nav.NavigateTo("/login");
    }

    private static string Initial(string label) =>
        string.IsNullOrEmpty(label) ? "?" : label[..1].ToUpperInvariant();
}
```

- [ ] **Step 2: Remove the dead scoped CSS file**

```bash
git rm src/Ruptura.Web/Layout/NavMenu.razor.css
```

- [ ] **Step 3: Append the sidebar-rail rules to `app.css`**

Replace the line:

```css
/* Tablet: handled by Task 2 (sidebar rail — depends on NavMenu markup) */
```

with:

```css
/* Sidebar icon rail — 600–1023px collapses text, keeps a monogram medallion
   per item; the native title="" attribute on each link supplies the tooltip. */
@media (min-width: 600px) and (max-width: 1023px) {
    .sidebar { width: var(--nav-width-rail); transform: none; }
    .main-wrapper { margin-left: var(--nav-width-rail); }
    .top-bar { left: var(--nav-width-rail); }

    .brand-tagline,
    .nav-section-label,
    .nav-link-text { display: none; }

    .sidebar-brand { padding: 1.25rem 0.5rem; text-align: center; }
    .sidebar-nav a.nav-link,
    .nav-btn-link { justify-content: center; padding: 0.65rem; }

    .nav-link-mono { display: flex; }

    .sidebar-footer { padding: 0.75rem 0.4rem; }
    .language-switcher { flex-direction: column; }
}

.nav-link-mono {
    display: none;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    width: 26px;
    height: 26px;
    border: 1px solid var(--sidebar-link);
    border-radius: 50%;
    font-family: var(--font-display);
    font-size: var(--text-2xs);
    color: var(--sidebar-link);
}
.nav-link.active .nav-link-mono {
    border-color: var(--accent-on-dark);
    color: #fff;
}
```

- [ ] **Step 4: Add the `Nav.Section.GameMaster` key**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Nav.Campaigns` line (`<data name="Nav.Campaigns"><value>Campaigns</value></data>`) and add directly above it:

```xml
  <data name="Nav.Section.GameMaster"><value>Game Master</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Nav.Campaigns` line and add directly above it:

```xml
  <data name="Nav.Section.GameMaster"><value>Mestre</value></data>
```

This also fixes a pre-existing bug: the section label was a hardcoded Portuguese string ("Mestre") shown even when the UI is set to English.

- [ ] **Step 5: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Layout/NavMenu.razor src/Ruptura.Web/wwwroot/css/app.css \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git rm --cached src/Ruptura.Web/Layout/NavMenu.razor.css 2>/dev/null || true
git commit -m "feat: collapse sidebar to an icon rail on tablet widths"
```

---

## Task 3: ThemeSwitcher — icons instead of L/S/D letters, localized labels

**Files:**
- Modify: `src/Ruptura.Web/Layout/ThemeSwitcher.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `.theme-switcher`/`.theme-btn` CSS (Task 1), `ThemeService.Current`/`ThemeService.SetAsync(ThemeMode)` (pre-existing, unchanged).

The original component's `title="Light"`/`"System"`/`"Dark"`/`"Theme"` were hardcoded English words, never localized — that's fixed here alongside the icon change, since the file is already being touched.

- [ ] **Step 1: Add resource keys**

In `AppStrings.resx`, after the `Common.Error` line (`<data name="Common.Error">...`), add:

```xml
  <data name="Theme.Switcher.Label"><value>Theme</value></data>
  <data name="Theme.Light"><value>Light</value></data>
  <data name="Theme.System"><value>System</value></data>
  <data name="Theme.Dark"><value>Dark</value></data>
```

In `AppStrings.pt-BR.resx`, after the matching `Common.Error` line, add:

```xml
  <data name="Theme.Switcher.Label"><value>Tema</value></data>
  <data name="Theme.Light"><value>Claro</value></data>
  <data name="Theme.System"><value>Sistema</value></data>
  <data name="Theme.Dark"><value>Escuro</value></data>
```

- [ ] **Step 2: Replace `ThemeSwitcher.razor`**

```razor
@inject ThemeService ThemeService
@inject IStringLocalizer<AppStrings> L

<div class="theme-switcher" role="group" aria-label="@L["Theme.Switcher.Label"]">
    <button class="theme-btn @(ThemeService.Current == ThemeMode.Light  ? "active" : "")"
            @onclick='() => SetAsync(ThemeMode.Light)' title="@L["Theme.Light"]" aria-label="@L["Theme.Light"]">
        <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
            <circle cx="10" cy="10" r="3.5" />
            <path d="M10 1.5v2M10 16.5v2M3.5 3.5l1.4 1.4M15.1 15.1l1.4 1.4M1.5 10h2M16.5 10h2M3.5 16.5l1.4-1.4M15.1 4.9l1.4-1.4" />
        </svg>
    </button>
    <button class="theme-btn @(ThemeService.Current == ThemeMode.System ? "active" : "")"
            @onclick='() => SetAsync(ThemeMode.System)' title="@L["Theme.System"]" aria-label="@L["Theme.System"]">
        <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
            <rect x="2.5" y="3.5" width="15" height="10" rx="1" />
            <path d="M7 17h6M10 13.5V17" />
        </svg>
    </button>
    <button class="theme-btn @(ThemeService.Current == ThemeMode.Dark   ? "active" : "")"
            @onclick='() => SetAsync(ThemeMode.Dark)' title="@L["Theme.Dark"]" aria-label="@L["Theme.Dark"]">
        <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
            <path d="M16.5 12.5A7 7 0 0 1 7.5 3.5a7 7 0 1 0 9 9Z" />
        </svg>
    </button>
</div>

@code {
    private async Task SetAsync(ThemeMode mode)
    {
        await ThemeService.SetAsync(mode);
        StateHasChanged();
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Layout/ThemeSwitcher.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: replace theme-switcher letters with icons and localize labels"
```

---

## Task 4: ToastService — toast queue (TDD)

**Files:**
- Modify: `tests/Ruptura.UnitTests/Ruptura.UnitTests.csproj`
- Create: `src/Ruptura.Web/Services/ToastService.cs`
- Test: `tests/Ruptura.UnitTests/Web/ToastServiceTests.cs`

**Interfaces:**
- Produces: `enum ToastLevel { Success, Error, Info }`, `record ToastMessage(Guid Id, string Text, ToastLevel Level)`, `class ToastService` with `event Action? OnChange`, `IReadOnlyList<ToastMessage> Messages`, `void Show(string text, ToastLevel level = ToastLevel.Info)`, `void Success(string text)`, `void Error(string text)`, `void Dismiss(Guid id)`. Consumed by Task 5 (`ToastContainer`) and Task 12 (`GmCatalog`).

- [ ] **Step 1: Add the `Ruptura.Web` project reference to the test project**

In `tests/Ruptura.UnitTests/Ruptura.UnitTests.csproj`, inside the existing `<ItemGroup>` that lists `ProjectReference`s, add:

```xml
    <ProjectReference Include="..\..\src\Ruptura.Web\Ruptura.Web.csproj" />
```

- [ ] **Step 2: Write the failing test**

```csharp
using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ToastServiceTests
{
    [Fact]
    public void Show_AddsMessageAndRaisesOnChange()
    {
        var sut = new ToastService();
        var raised = false;
        sut.OnChange += () => raised = true;

        sut.Show("Saved", ToastLevel.Success);

        sut.Messages.Should().ContainSingle(m => m.Text == "Saved" && m.Level == ToastLevel.Success);
        raised.Should().BeTrue();
    }

    [Fact]
    public void Success_UsesSuccessLevel()
    {
        var sut = new ToastService();

        sut.Success("Done");

        sut.Messages.Single().Level.Should().Be(ToastLevel.Success);
    }

    [Fact]
    public void Error_UsesErrorLevel()
    {
        var sut = new ToastService();

        sut.Error("Failed");

        sut.Messages.Single().Level.Should().Be(ToastLevel.Error);
    }

    [Fact]
    public void Dismiss_RemovesMessageById_AndRaisesOnChange()
    {
        var sut = new ToastService();
        sut.Show("Bye");
        var id = sut.Messages.Single().Id;
        var raised = false;
        sut.OnChange += () => raised = true;

        sut.Dismiss(id);

        sut.Messages.Should().BeEmpty();
        raised.Should().BeTrue();
    }

    [Fact]
    public void Dismiss_UnknownId_DoesNothing()
    {
        var sut = new ToastService();
        sut.Show("Stays");

        sut.Dismiss(Guid.NewGuid());

        sut.Messages.Should().ContainSingle();
    }
}
```

- [ ] **Step 3: Run to verify it fails**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~ToastServiceTests`
Expected: FAIL (build error — `Ruptura.Web.Services.ToastService` does not exist yet).

- [ ] **Step 4: Implement `ToastService`**

```csharp
namespace Ruptura.Web.Services;

public enum ToastLevel { Success, Error, Info }

public sealed record ToastMessage(Guid Id, string Text, ToastLevel Level);

public class ToastService
{
    private readonly List<ToastMessage> _messages = [];

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Messages => _messages;

    public void Show(string text, ToastLevel level = ToastLevel.Info)
    {
        _messages.Add(new ToastMessage(Guid.NewGuid(), text, level));
        OnChange?.Invoke();
    }

    public void Success(string text) => Show(text, ToastLevel.Success);

    public void Error(string text) => Show(text, ToastLevel.Error);

    public void Dismiss(Guid id)
    {
        _messages.RemoveAll(m => m.Id == id);
        OnChange?.Invoke();
    }
}
```

- [ ] **Step 5: Run to verify it passes**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~ToastServiceTests`
Expected: PASS (5/5).

- [ ] **Step 6: Register the service in DI**

In `src/Ruptura.Web/Program.cs`, next to the existing `builder.Services.AddScoped<ThemeService>();` line, add:

```csharp
builder.Services.AddScoped<ToastService>();
```

- [ ] **Step 7: Commit**

```bash
git add tests/Ruptura.UnitTests/Ruptura.UnitTests.csproj tests/Ruptura.UnitTests/Web/ToastServiceTests.cs \
        src/Ruptura.Web/Services/ToastService.cs src/Ruptura.Web/Program.cs
git commit -m "feat: add ToastService with unit tests"
```

---

## Task 5: ToastContainer — renders the toast stack, auto-dismiss

**Files:**
- Create: `src/Ruptura.Web/Layout/ToastContainer.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService.Messages`, `ToastService.OnChange`, `ToastService.Dismiss(Guid)` (Task 4); `.toast-stack`/`.toast-item`/`.toast-close` CSS (Task 1).
- Produces: `<ToastContainer />` — mounted once in `MainLayout` (Task 8).

- [ ] **Step 1: Add the `Common.Close` key**

In `AppStrings.resx`, after `Common.Error`, add:

```xml
  <data name="Common.Close"><value>Close</value></data>
```

In `AppStrings.pt-BR.resx`, after the matching `Common.Error` line, add:

```xml
  <data name="Common.Close"><value>Fechar</value></data>
```

- [ ] **Step 2: Create `ToastContainer.razor`**

```razor
@inject ToastService Toast
@inject IStringLocalizer<AppStrings> L
@implements IDisposable

<div class="toast-stack" aria-live="polite" aria-atomic="false">
    @foreach (var msg in Toast.Messages)
    {
        <div class="toast-item toast-@msg.Level.ToString().ToLowerInvariant()" role="status">
            <span class="toast-text">@msg.Text</span>
            <button class="toast-close" aria-label="@L["Common.Close"]" @onclick="() => Toast.Dismiss(msg.Id)">×</button>
        </div>
    }
</div>

@code {
    private const int AutoDismissMs = 5000;
    private readonly HashSet<Guid> _scheduled = [];

    protected override void OnInitialized() => Toast.OnChange += HandleChange;

    private void HandleChange()
    {
        foreach (var msg in Toast.Messages)
        {
            if (_scheduled.Add(msg.Id))
            {
                _ = AutoDismissAsync(msg.Id);
            }
        }
        InvokeAsync(StateHasChanged);
    }

    private async Task AutoDismissAsync(Guid id)
    {
        await Task.Delay(AutoDismissMs);
        Toast.Dismiss(id);
        _scheduled.Remove(id);
    }

    public void Dispose() => Toast.OnChange -= HandleChange;
}
```

`_scheduled` guarantees each message gets exactly one auto-dismiss timer even though `OnChange` fires again on every dismissal (adding a toast and removing one both go through the same event).

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.` (`ToastContainer` isn't referenced anywhere yet — that's Task 8 — so this only checks it compiles standalone.)

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Layout/ToastContainer.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add ToastContainer component"
```

---

## Task 6: ConfirmService — confirmation flow (TDD)

**Files:**
- Create: `src/Ruptura.Web/Services/ConfirmService.cs`
- Test: `tests/Ruptura.UnitTests/Web/ConfirmServiceTests.cs`

**Interfaces:**
- Produces: `record ConfirmRequest(string Title, string Message, string ConfirmLabel, string CancelLabel)`, `class ConfirmService` with `event Action? OnChange`, `ConfirmRequest? Current`, `Task<bool> AskAsync(string title, string message, string confirmLabel, string cancelLabel)`, `void Resolve(bool result)`. Consumed by Task 7 (`ConfirmDialog`), Task 8 (`MainLayout` global Escape), and Task 12 (`GmCatalog`).

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ConfirmServiceTests
{
    [Fact]
    public void AskAsync_SetsCurrentRequest_AndRaisesOnChange()
    {
        var sut = new ConfirmService();
        var raised = false;
        sut.OnChange += () => raised = true;

        _ = sut.AskAsync("Title", "Message", "Yes", "No");

        sut.Current.Should().NotBeNull();
        sut.Current!.Title.Should().Be("Title");
        raised.Should().BeTrue();
    }

    [Fact]
    public async Task Resolve_True_CompletesTaskWithTrue_AndClearsCurrent()
    {
        var sut = new ConfirmService();
        var task = sut.AskAsync("Delete?", "Sure?", "Delete", "Cancel");

        sut.Resolve(true);

        (await task).Should().BeTrue();
        sut.Current.Should().BeNull();
    }

    [Fact]
    public async Task Resolve_False_CompletesTaskWithFalse()
    {
        var sut = new ConfirmService();
        var task = sut.AskAsync("Delete?", "Sure?", "Delete", "Cancel");

        sut.Resolve(false);

        (await task).Should().BeFalse();
    }

    [Fact]
    public async Task AskAsync_CalledAgainBeforeResolve_CancelsThePriorRequestAsFalse()
    {
        var sut = new ConfirmService();
        var first = sut.AskAsync("First", "m", "Yes", "No");

        var second = sut.AskAsync("Second", "m", "Yes", "No");

        (await first).Should().BeFalse();
        sut.Current!.Title.Should().Be("Second");
        _ = second;
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~ConfirmServiceTests`
Expected: FAIL (build error — `Ruptura.Web.Services.ConfirmService` does not exist yet).

- [ ] **Step 3: Implement `ConfirmService`**

```csharp
namespace Ruptura.Web.Services;

public sealed record ConfirmRequest(string Title, string Message, string ConfirmLabel, string CancelLabel);

public class ConfirmService
{
    private TaskCompletionSource<bool>? _pending;

    public event Action? OnChange;

    public ConfirmRequest? Current { get; private set; }

    public Task<bool> AskAsync(string title, string message, string confirmLabel, string cancelLabel)
    {
        _pending?.TrySetResult(false); // an unresolved prior request is treated as cancelled
        _pending = new TaskCompletionSource<bool>();
        Current = new ConfirmRequest(title, message, confirmLabel, cancelLabel);
        OnChange?.Invoke();
        return _pending.Task;
    }

    public void Resolve(bool result)
    {
        var pending = _pending;
        Current = null;
        _pending = null;
        OnChange?.Invoke();
        pending?.TrySetResult(result);
    }
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~ConfirmServiceTests`
Expected: PASS (4/4).

- [ ] **Step 5: Register the service in DI**

In `src/Ruptura.Web/Program.cs`, next to `builder.Services.AddScoped<ToastService>();`, add:

```csharp
builder.Services.AddScoped<ConfirmService>();
```

- [ ] **Step 6: Commit**

```bash
git add tests/Ruptura.UnitTests/Web/ConfirmServiceTests.cs src/Ruptura.Web/Services/ConfirmService.cs src/Ruptura.Web/Program.cs
git commit -m "feat: add ConfirmService with unit tests"
```

---

## Task 7: ConfirmDialog — renders the confirmation modal

**Files:**
- Create: `src/Ruptura.Web/Layout/ConfirmDialog.razor`

**Interfaces:**
- Consumes: `ConfirmService.Current`, `ConfirmService.OnChange`, `ConfirmService.Resolve(bool)` (Task 6); `.btn-danger` (Task 1), `.confirm-overlay`/`.confirm-box`/`.confirm-title`/`.confirm-message`/`.confirm-actions` CSS (Task 1).
- Produces: `<ConfirmDialog />` — mounted once in `MainLayout` (Task 8).

- [ ] **Step 1: Create `ConfirmDialog.razor`**

```razor
@inject ConfirmService Confirm
@implements IDisposable

@if (Confirm.Current is { } request)
{
    <div class="confirm-overlay" @onclick="() => Confirm.Resolve(false)">
        <div class="confirm-box" @onclick:stopPropagation="true" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title">
            <h2 class="confirm-title" id="confirm-title">@request.Title</h2>
            <p class="confirm-message">@request.Message</p>
            <div class="confirm-actions">
                <button class="btn btn-outline-secondary btn-sm" @onclick="() => Confirm.Resolve(false)">@request.CancelLabel</button>
                <button class="btn btn-danger btn-sm" @onclick="() => Confirm.Resolve(true)">@request.ConfirmLabel</button>
            </div>
        </div>
    </div>
}

@code {
    protected override void OnInitialized() => Confirm.OnChange += HandleChange;

    private void HandleChange() => InvokeAsync(StateHasChanged);

    public void Dispose() => Confirm.OnChange -= HandleChange;
}
```

Clicking the overlay resolves `false` (dismiss-by-clicking-outside); `@onclick:stopPropagation` on the box itself keeps clicks inside the dialog from bubbling up and closing it.

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add src/Ruptura.Web/Layout/ConfirmDialog.razor
git commit -m "feat: add ConfirmDialog component"
```

---

## Task 8: Wire Toast/Confirm into MainLayout + global Escape key

**Files:**
- Modify: `src/Ruptura.Web/Layout/MainLayout.razor`
- Modify: `src/Ruptura.Web/wwwroot/js/ruptura.js`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `<ToastContainer />` (Task 5), `<ConfirmDialog />` + `ConfirmService.Current`/`Resolve(bool)` (Tasks 6–7).
- Produces: `ruptura.bindGlobalEscape(dotNetRef)` / `ruptura.unbindGlobalEscape()` JS functions any future layout-level component can reuse; `MainLayout.OnGlobalEscape()` `[JSInvokable]` entry point.

Escape now closes, in priority order: an open confirmation dialog first, then the mobile sidebar overlay. Both live in `MainLayout`, so a single global listener there (rather than one per component) keeps the "what does Escape do right now" logic in one place.

- [ ] **Step 1: Add the `Nav.ToggleMenu` key**

In `AppStrings.resx`, after `Common.Close` (added in Task 5), add:

```xml
  <data name="Nav.ToggleMenu"><value>Toggle menu</value></data>
```

In `AppStrings.pt-BR.resx`, after the matching line, add:

```xml
  <data name="Nav.ToggleMenu"><value>Alternar menu</value></data>
```

- [ ] **Step 2: Append the keyboard-shortcut helpers to `ruptura.js`**

In `src/Ruptura.Web/wwwroot/js/ruptura.js`, the `window.ruptura` object currently ends with:

```js
    copyToClipboard: async function (text) {
        if (window.isSecureContext && navigator.clipboard) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch {
                // fall through to legacy fallback
            }
        }

        try {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.left = '-9999px';
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            const success = document.execCommand('copy');
            document.body.removeChild(textarea);
            return success;
        } catch {
            return false;
        }
    }
};
```

Replace the closing `    }\n};` (the end of `copyToClipboard`) with:

```js
    },

    // ── Keyboard shortcuts ───────────────────────────────────────────────────

    bindGlobalEscape: function (dotNetRef) {
        const handler = function (e) {
            if (e.key === 'Escape') {
                dotNetRef.invokeMethodAsync('OnGlobalEscape');
            }
        };
        document.addEventListener('keydown', handler);
        window._rupturaEscapeHandler = handler;
    },

    unbindGlobalEscape: function () {
        if (window._rupturaEscapeHandler) {
            document.removeEventListener('keydown', window._rupturaEscapeHandler);
            delete window._rupturaEscapeHandler;
        }
    },

    bindSearchShortcut: function (inputElement) {
        const handler = function (e) {
            const active = document.activeElement;
            const typing = active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.isContentEditable);
            if (typing) return;
            if (e.key === '/' || (e.key.toLowerCase() === 'k' && (e.ctrlKey || e.metaKey))) {
                e.preventDefault();
                inputElement.focus();
            }
        };
        document.addEventListener('keydown', handler);
        inputElement._rupturaShortcutHandler = handler;
    },

    unbindSearchShortcut: function (inputElement) {
        if (inputElement && inputElement._rupturaShortcutHandler) {
            document.removeEventListener('keydown', inputElement._rupturaShortcutHandler);
            delete inputElement._rupturaShortcutHandler;
        }
    }
};
```

(`bindSearchShortcut`/`unbindSearchShortcut` are used by Task 9's `TableSearchBox` — added here since both belong to the same "keyboard shortcuts" section of the file.)

- [ ] **Step 3: Replace `MainLayout.razor`**

```razor
@inherits LayoutComponentBase
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject ThemeService ThemeService
@inject ConfirmService ConfirmSvc
@inject IJSRuntime JS
@inject AuthenticationStateProvider AuthStateProvider
@implements IAsyncDisposable

<div class="app-shell">

    <!-- Sidebar overlay (mobile) -->
    @if (_sidebarOpen)
    {
        <div style="position:fixed;inset:0;background:rgba(0,0,0,0.5);z-index:199"
             @onclick="() => _sidebarOpen = false"></div>
    }

    <!-- Sidebar -->
    <aside class="sidebar @(_sidebarOpen ? "open" : "")">
        <a class="sidebar-brand" href="/">
            <span class="brand-name">RUPTURA</span>
            <span class="brand-tagline">Campaign Registry</span>
        </a>
        <NavMenu />
        <div class="sidebar-footer">
            <LanguageSwitcher />
        </div>
    </aside>

    <!-- Topbar (sibling of sidebar, both fixed) -->
    <header class="top-bar">
        <button class="hamburger" @onclick="() => _sidebarOpen = !_sidebarOpen"
                aria-label="@L["Nav.ToggleMenu"]">
            <span></span><span></span><span></span>
        </button>
        <span class="top-bar-title">@_pageTitle</span>
        <div class="top-bar-controls">
            <ThemeSwitcher />
            @if (!string.IsNullOrEmpty(_displayName))
            {
                <div class="user-chip">
                    <span class="user-chip-name">@_displayName</span>
                    @if (!string.IsNullOrEmpty(_role))
                    {
                        <span class="user-role-badge">@_role</span>
                    }
                </div>
            }
        </div>
    </header>

    <!-- Scrollable content area -->
    <main class="main-wrapper">
        @Body
    </main>

    <ToastContainer />
    <ConfirmDialog />

</div>

@code {
    private bool _sidebarOpen;
    private string _pageTitle = string.Empty;
    private string _displayName = string.Empty;
    private string _role = string.Empty;
    private DotNetObjectReference<MainLayout>? _selfRef;

    protected override async Task OnInitializedAsync()
    {
        await ThemeService.InitAsync();
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        _displayName = state.User.FindFirst("name")?.Value ?? string.Empty;
        _role        = state.User.FindFirst("role")?.Value ?? string.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("ruptura.bindGlobalEscape", _selfRef);
        }
    }

    [JSInvokable]
    public void OnGlobalEscape()
    {
        if (ConfirmSvc.Current is not null)
        {
            ConfirmSvc.Resolve(false);
        }
        else if (_sidebarOpen)
        {
            _sidebarOpen = false;
        }
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_selfRef is not null)
        {
            try
            {
                await JS.InvokeVoidAsync("ruptura.unbindGlobalEscape");
            }
            catch (JSDisconnectedException)
            {
                // circuit already gone — nothing to clean up client-side
            }
            _selfRef.Dispose();
        }
    }
}
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Layout/MainLayout.razor src/Ruptura.Web/wwwroot/js/ruptura.js \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: mount toast/confirm UI and wire global Escape handling"
```

---

## Task 9: TableFilter (TDD) + TableSearchBox

**Files:**
- Create: `src/Ruptura.Web/Shared/TableFilter.cs`
- Create: `src/Ruptura.Web/Shared/TableSearchBox.razor`
- Test: `tests/Ruptura.UnitTests/Web/TableFilterTests.cs`
- Modify: `src/Ruptura.Web/_Imports.razor`

**Interfaces:**
- Produces: `static class TableFilter { static bool Matches(string? term, params string?[] fields) }` — consumed directly by any page (Task 12: `GmCatalog`). `TableSearchBox` component with `[Parameter] string Value`, `[Parameter] EventCallback<string> ValueChanged`, `[Parameter] string Placeholder` — supports `@bind-Value`.

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Ruptura.Web.Shared;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class TableFilterTests
{
    [Fact]
    public void Matches_ReturnsTrue_WhenTermIsNullOrWhitespace()
    {
        TableFilter.Matches(null, "Anything").Should().BeTrue();
        TableFilter.Matches("   ", "Anything").Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenAnyFieldContainsTermCaseInsensitive()
    {
        TableFilter.Matches("gob", "Goblin", "A short humanoid").Should().BeTrue();
        TableFilter.Matches("HUMANOID", "Goblin", "A short humanoid").Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenNoFieldContainsTerm()
    {
        TableFilter.Matches("dragon", "Goblin", "A short humanoid").Should().BeFalse();
    }

    [Fact]
    public void Matches_IgnoresNullFields()
    {
        TableFilter.Matches("gob", null, "Goblin").Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~TableFilterTests`
Expected: FAIL (build error — `Ruptura.Web.Shared.TableFilter` does not exist yet).

- [ ] **Step 3: Implement `TableFilter`**

```csharp
namespace Ruptura.Web.Shared;

public static class TableFilter
{
    public static bool Matches(string? term, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(term)) return true;

        foreach (var field in fields)
        {
            if (field is not null && field.Contains(term, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~TableFilterTests`
Expected: PASS (4/4).

- [ ] **Step 5: Add `Ruptura.Web.Shared` to `_Imports.razor`**

In `src/Ruptura.Web/_Imports.razor`, next to `@using Ruptura.Web.Services`, add:

```razor
@using Ruptura.Web.Shared
```

- [ ] **Step 6: Create `TableSearchBox.razor`**

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

<div class="table-search">
    <input @ref="_inputRef"
           class="form-control table-search-input"
           type="search"
           placeholder="@Placeholder"
           value="@Value"
           @oninput="OnInputAsync" />
</div>

@code {
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    private ElementReference _inputRef;

    private Task OnInputAsync(ChangeEventArgs e) =>
        ValueChanged.InvokeAsync(e.Value?.ToString() ?? string.Empty);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("ruptura.bindSearchShortcut", _inputRef);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("ruptura.unbindSearchShortcut", _inputRef);
        }
        catch (JSDisconnectedException)
        {
            // circuit already gone — nothing to clean up client-side
        }
    }
}
```

- [ ] **Step 7: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
git add tests/Ruptura.UnitTests/Web/TableFilterTests.cs src/Ruptura.Web/Shared/TableFilter.cs \
        src/Ruptura.Web/Shared/TableSearchBox.razor src/Ruptura.Web/_Imports.razor
git commit -m "feat: add TableFilter and TableSearchBox with Ctrl+K/ / shortcut"
```

---

## Task 10: LoadingIndicator + SkeletonRows

**Files:**
- Create: `src/Ruptura.Web/Shared/LoadingIndicator.razor`
- Create: `src/Ruptura.Web/Shared/SkeletonRows.razor`

**Interfaces:**
- Produces: `LoadingIndicator` with `[Parameter] string Text` — consumed by Task 12 (`GmCatalog`). `SkeletonRows` with `[Parameter] int Rows = 4`, `[Parameter] int Columns = 3` (renders `<tr>` elements — must be placed inside a `<tbody>`) — not wired into any page in this plan (`GmCatalog`'s one loading state uses `LoadingIndicator` instead); built and unit-verified now so it's ready for a page with a table-shaped loading state in a follow-up page-group plan.

Both are pure presentation (no branching logic worth a unit test); verified by build + the visual pass in Task 13.

- [ ] **Step 1: Create `LoadingIndicator.razor`**

```razor
<div class="loading-inline" role="status">
    <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
    <span>@Text</span>
</div>

@code {
    [Parameter] public string Text { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create `SkeletonRows.razor`**

```razor
@for (var r = 0; r < Rows; r++)
{
    <tr class="skeleton-row" aria-hidden="true">
        @for (var c = 0; c < Columns; c++)
        {
            <td><span class="skeleton-bar"></span></td>
        }
    </tr>
}

@code {
    [Parameter] public int Rows { get; set; } = 4;
    [Parameter] public int Columns { get; set; } = 3;
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Shared/LoadingIndicator.razor src/Ruptura.Web/Shared/SkeletonRows.razor
git commit -m "feat: add LoadingIndicator and SkeletonRows components"
```

---

## Task 11: Breadcrumbs

**Files:**
- Create: `src/Ruptura.Web/Shared/BreadcrumbItem.cs`
- Create: `src/Ruptura.Web/Shared/Breadcrumbs.razor`

**Interfaces:**
- Produces: `record BreadcrumbItem(string Text, string? Href)`; `Breadcrumbs` component with `[Parameter, EditorRequired] List<BreadcrumbItem> Items`. Not wired into any page in this plan (no nested-page flow is touched here) — first live usage lands with the GM page-group rollout (spec §8, Fase 3.3, e.g. Campaign → Character Sheet), which is a follow-up plan. Built and verified now so it's ready to use.

- [ ] **Step 1: Create `BreadcrumbItem.cs`**

```csharp
namespace Ruptura.Web.Shared;

public sealed record BreadcrumbItem(string Text, string? Href);
```

- [ ] **Step 2: Create `Breadcrumbs.razor`**

```razor
@if (Items is { Count: > 0 })
{
    <nav class="breadcrumbs" aria-label="Breadcrumb">
        @for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var isLast = i == Items.Count - 1;
            <span class="breadcrumb-item">
                @if (!isLast && item.Href is not null)
                {
                    <a href="@item.Href">@item.Text</a>
                }
                else
                {
                    <span aria-current="page">@item.Text</span>
                }
            </span>
            @if (!isLast)
            {
                <span class="breadcrumb-sep" aria-hidden="true">/</span>
            }
        }
    </nav>
}

@code {
    [Parameter, EditorRequired] public List<BreadcrumbItem> Items { get; set; } = [];
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Shared/BreadcrumbItem.cs src/Ruptura.Web/Shared/Breadcrumbs.razor
git commit -m "feat: add Breadcrumbs component"
```

---

## Task 12: Reference implementation — wire the toolkit into GmCatalog

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmCatalog.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService` (Task 4), `ConfirmService` (Task 6), `TableFilter`/`TableSearchBox` (Task 9), `LoadingIndicator` (Task 10), `.ledger-table.stack-mobile` + `data-label` (Task 1).

This is the concrete example the rest of the app's page-group rollout (spec §8, Fase 3) copies: confirm before delete, toast on every create/save/delete outcome, a shared loading indicator instead of the inline spinner markup, instant client-side search, and a mobile card-table.

- [ ] **Step 1: Add new resource keys**

In `AppStrings.resx`, find the `Gm.Catalog.Archived` line and add directly after it:

```xml
  <data name="Gm.Catalog.SearchPlaceholder"><value>Search catalog…</value></data>
  <data name="Gm.Catalog.DeleteConfirm.Title"><value>Delete entry?</value></data>
  <data name="Gm.Catalog.DeleteConfirm.Message"><value>This will permanently delete "{0}". This cannot be undone.</value></data>
  <data name="Gm.Catalog.DeleteSuccess"><value>Entry deleted.</value></data>
  <data name="Gm.Catalog.CreateSuccess"><value>Entry created.</value></data>
  <data name="Gm.Catalog.SaveSuccess"><value>Entry saved.</value></data>
```

In `AppStrings.pt-BR.resx`, find the matching `Gm.Catalog.Archived` line and add directly after it:

```xml
  <data name="Gm.Catalog.SearchPlaceholder"><value>Buscar catálogo…</value></data>
  <data name="Gm.Catalog.DeleteConfirm.Title"><value>Excluir entrada?</value></data>
  <data name="Gm.Catalog.DeleteConfirm.Message"><value>Isso vai excluir "{0}" permanentemente. Essa ação não pode ser desfeita.</value></data>
  <data name="Gm.Catalog.DeleteSuccess"><value>Entrada excluída.</value></data>
  <data name="Gm.Catalog.CreateSuccess"><value>Entrada criada.</value></data>
  <data name="Gm.Catalog.SaveSuccess"><value>Entrada salva.</value></data>
```

- [ ] **Step 2: Replace `GmCatalog.razor`**

```razor
@page "/gm/campaigns/{CampaignId:guid}/catalog"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService
@inject ToastService Toast
@inject ConfirmService Confirm

<PageTitle>@L["Gm.Catalog.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.Catalog.Title"]</h1>
    </div>

    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }

    <div class="section-header">
        <span class="section-title">@L["Gm.Catalog.TypeLabel"]</span>
        <select class="form-select" style="width:220px" value="@_selectedType" @onchange="OnTypeChanged">
            @foreach (var type in Types)
            {
                <option value="@type">@type</option>
            }
        </select>
    </div>

    @if (_loading)
    {
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_entries.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Catalog.Empty"]</p>
        </div>
    }
    else
    {
        <TableSearchBox @bind-Value="_searchTerm" Placeholder="@L["Gm.Catalog.SearchPlaceholder"]" />

        @if (FilteredEntries.Count == 0)
        {
            <div class="ledger-empty">
                <p>@L["Gm.Catalog.Empty"]</p>
            </div>
        }
        else
        {
            <div class="ledger-table-wrap">
                <table class="ledger-table stack-mobile">
                    <thead>
                        <tr>
                            <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                            <th>@L["Gm.Catalog.TypeLabel"]</th>
                            <th>DataJson</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var entry in FilteredEntries)
                        {
                            <tr>
                                <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@entry.Name</td>
                                <td data-label="@L["Gm.Catalog.TypeLabel"]">@(entry.IsGlobal ? L["Gm.Catalog.Official"] : L["Gm.Catalog.Homebrew"])</td>
                                <td data-label="DataJson" style="color:var(--text-muted);font-size:var(--text-sm);max-width:320px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">
                                    @entry.DataJson
                                </td>
                                <td data-label="">
                                    @if (entry.IsArchived)
                                    {
                                        <span style="color:var(--text-muted)">@L["Gm.Catalog.Archived"]</span>
                                    }
                                    else if (!entry.IsGlobal)
                                    {
                                        <button class="btn btn-outline-secondary btn-sm" @onclick="() => StartEdit(entry)">@L["Gm.Catalog.Edit"]</button>
                                        <button class="btn btn-outline-secondary btn-sm" @onclick="() => DeleteAsync(entry.Id, entry.Name)">@L["Gm.Catalog.Delete"]</button>
                                    }
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    }

    <div class="section-header" style="margin-top:2rem">
        <span class="section-title">@(_editingId is null ? L["Gm.Catalog.Create"] : L["Gm.Catalog.Edit"])</span>
    </div>
    <div style="display:flex;flex-direction:column;gap:.75rem;max-width:480px">
        <input class="form-control" placeholder="@L["Gm.Catalog.NamePlaceholder"]" @bind="_formName" @bind:event="oninput" />
        <textarea class="form-control" rows="4" placeholder="@L["Gm.Catalog.DataJsonPlaceholder"]" @bind="_formDataJson" @bind:event="oninput"></textarea>
        <div style="display:flex;gap:.5rem">
            <button class="btn btn-primary btn-sm" @onclick="SaveAsync" disabled="@(_saving || string.IsNullOrWhiteSpace(_formName))">
                @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @(_editingId is null ? L["Gm.Catalog.Create"] : L["Gm.Catalog.Save"])
            </button>
            @if (_editingId is not null)
            {
                <button class="btn btn-outline-secondary btn-sm" @onclick="CancelEdit">@L["Gm.Catalog.Cancel"]</button>
            }
        </div>
    </div>
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }

    private static readonly string[] Types =
    [
        "Origin", "Background", "Lineage", "Aptitude", "Talent",
        "Skill", "Spell", "Technique", "EquipmentItem"
    ];

    private string _selectedType = Types[0];
    private List<CatalogEntryResponse> _entries = [];
    private bool _loading = true;
    private bool _saving;
    private Guid? _editingId;
    private string _formName = string.Empty;
    private string _formDataJson = "{}";
    private string? _errorMessage;
    private string _searchTerm = string.Empty;

    private List<CatalogEntryResponse> FilteredEntries =>
        _entries.Where(e => TableFilter.Matches(_searchTerm, e.Name, e.DataJson)).ToList();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task OnTypeChanged(ChangeEventArgs e)
    {
        _selectedType = e.Value?.ToString() ?? Types[0];
        CancelEdit();
        _searchTerm = string.Empty;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _errorMessage = null;
        var result = await CatalogService.GetByTypeAsync(_selectedType, CampaignId, includeArchived: true);
        if (result is null)
        {
            _errorMessage = L["Common.Error"];
            _entries = [];
        }
        else
        {
            _entries = result.Data?.ToList() ?? [];
        }
        _loading = false;
    }

    private void StartEdit(CatalogEntryResponse entry)
    {
        _editingId = entry.Id;
        _formName = entry.Name;
        _formDataJson = entry.DataJson;
    }

    private void CancelEdit()
    {
        _editingId = null;
        _formName = string.Empty;
        _formDataJson = "{}";
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_formName)) return;

        _saving = true;
        _errorMessage = null;
        var wasCreate = _editingId is null;

        var result = wasCreate
            ? await CatalogService.CreateAsync(new CreateCatalogEntryRequest
            {
                CampaignId = CampaignId, Type = _selectedType, Name = _formName, DataJson = _formDataJson
            })
            : await CatalogService.UpdateAsync(_editingId!.Value, new UpdateCatalogEntryRequest
            {
                Name = _formName, DataJson = _formDataJson
            });

        if (result?.Data is not null)
        {
            Toast.Success(wasCreate ? L["Gm.Catalog.CreateSuccess"] : L["Gm.Catalog.SaveSuccess"]);
            CancelEdit();
            await LoadAsync();
        }
        else
        {
            var message = result?.Message ?? L["Common.Error"];
            _errorMessage = message;
            Toast.Error(message);
        }

        _saving = false;
    }

    private async Task DeleteAsync(Guid id, string name)
    {
        var confirmed = await Confirm.AskAsync(
            L["Gm.Catalog.DeleteConfirm.Title"],
            L["Gm.Catalog.DeleteConfirm.Message", name],
            L["Gm.Catalog.Delete"],
            L["Gm.Catalog.Cancel"]);
        if (!confirmed) return;

        _errorMessage = null;
        var result = await CatalogService.DeleteAsync(id);
        if (result?.Success == true)
        {
            Toast.Success(L["Gm.Catalog.DeleteSuccess"]);
            await LoadAsync();
        }
        else
        {
            var message = result?.Message ?? L["Common.Error"];
            _errorMessage = message;
            Toast.Error(message);
        }
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCatalog.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire usability toolkit into GmCatalog as the reference implementation"
```

---

## Task 13: Final verification — full build, full test run, visual pass

**Files:** none (verification only)

**Interfaces:** none — this task confirms every earlier task's deliverable together.

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build`
Expected: `Build succeeded.` with 0 errors.

- [ ] **Step 2: Run the full unit test suite**

Run: `dotnet test tests/Ruptura.UnitTests`
Expected: all tests pass, including the 13 new ones from Tasks 4, 6, and 9.

- [ ] **Step 3: Launch the app and capture screenshots**

Use the `run` skill to build and serve `Ruptura.Web` (Docker via `make up`, or the local `dotnet` workflow from `CLAUDE.md` — whichever the skill's own detection picks). Capture screenshots of:

- `/login` — light and dark, desktop (≥1024px) and mobile (<600px) widths.
- `/dashboard` (as GM) — light and dark, desktop, tablet (600–1023px, confirms the icon rail), mobile.
- `/gm/campaigns/{id}/catalog` — desktop and mobile (confirms the search box, the mobile card-table via `data-label`, and — by triggering a delete — confirmation modal and toast).

Confirm for each: no text below 11px, no low-contrast text against its background, sidebar legible in both themes, tablet shows the icon rail (not the full sidebar, not the mobile overlay), mobile catalog table renders as stacked cards.

- [ ] **Step 4: Fix anything the visual pass surfaces**

If a screenshot shows a contrast, sizing, or layout regression, fix it in the relevant task's file (do not create a new one-off task for this — amend the CSS/component that owns the broken selector) and re-run Steps 1–3 until clean.

- [ ] **Step 5: Report**

Summarize what changed (tokens, typography, responsive tiers, the 6 new toolkit components, the `GmCatalog` reference implementation) and point to `docs/superpowers/specs/2026-08-06-design-system-rework-design.md` §8 for the remaining page-group rollout, which is out of scope for this plan and should be planned separately per group (Auth, GM, Player, Character Sheet).
