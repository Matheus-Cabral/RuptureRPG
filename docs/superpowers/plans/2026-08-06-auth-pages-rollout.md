# Auth Pages Rollout (Login, Register, Landing) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the design-system rework (merged in PR #2) to the Auth page group — `Login.razor`, `Register.razor`, `Index.razor` (landing) — by fixing the handful of page-local issues the shared CSS foundation doesn't already cover, then verifying the group visually across themes and breakpoints.

**Architecture:** No new components or services — this group needs none of the usability toolkit (no tables, no destructive actions, no multi-step async flows beyond the existing form-submit spinners). Because `Login`/`Register`/`Index` already consume shared classes from `wwwroot/css/app.css` (`.auth-scene`, `.auth-form-wrap`, `.landing-hero`, etc.), the color/typography/responsive rework from PR #2 already applies to them automatically. This plan closes the small remaining gaps: two hardcoded values these pages still carry, one deferred contrast bug in global CSS, and a visual verification pass to confirm nothing needs more.

**Tech Stack:** ASP.NET Core Blazor WebAssembly 8, plain CSS (custom properties), `IStringLocalizer<AppStrings>`.

## Global Constraints

- No font-size anywhere below `--text-2xs` (11px) (spec §4, `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`).
- Every user-facing string goes through `IStringLocalizer<AppStrings>` with a key in **both** `AppStrings.resx` (English) and `AppStrings.pt-BR.resx` (Portuguese) — no hardcoded text.
- All colors meet WCAG AA (≥4.5:1 normal text, ≥3:1 large text/UI) against the surface they render on.
- Follow the existing pattern: reuse shared `app.css` classes and `--*` tokens rather than introducing page-local styles, unless a page genuinely needs something the shared system doesn't cover.
- This plan does not introduce `ToastService`/`ConfirmService`/`TableSearchBox`/etc. — none of the three pages have a use case for them (no lists, no destructive actions).

**Full design spec:** `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`
**Foundation plan (already merged):** `docs/superpowers/plans/2026-08-06-design-system-rework.md`

---

## File Structure

```
src/Ruptura.Web/
├── wwwroot/css/app.css                 Modify — fix .blazor-error-boundary / #blazor-error-ui contrast (deferred from PR #2's final review)
├── Pages/
│   ├── Register.razor                  Modify — replace hardcoded inline font-size with a token
│   └── Index.razor                     Modify — localize the hardcoded "Sistema" eyebrow text
└── Resources/
    ├── AppStrings.resx                 Modify — new `Landing.Pillars.Eyebrow` key
    └── AppStrings.pt-BR.resx           Modify — new `Landing.Pillars.Eyebrow` key
```

`Login.razor`, `GmFields.razor`, and `PlayerFields.razor` were checked and need no changes — they already use only shared classes and tokens, with no hardcoded styles or untranslated strings.

---

## Task 1: Fix known Auth-group issues

**Files:**
- Modify: `src/Ruptura.Web/wwwroot/css/app.css:1116-1131`
- Modify: `src/Ruptura.Web/Pages/Register.razor:39`
- Modify: `src/Ruptura.Web/Pages/Index.razor:29`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `--danger-text` (already defined in `app.css` from PR #2 — light `#fff`, dark `#1A1511`, AA-verified against `--danger` for exactly this white-on-red contrast problem); `--text-sm` (already defined, 14px).
- Produces: nothing new for other tasks to consume — this task is a closed set of fixes.

This is a pure fix-and-verify task — no new test-worthy logic, so there's no TDD cycle here (same as the CSS-only tasks in the foundation plan). Verification is `dotnet build` plus this task's own visual/contrast check, and the group-wide visual pass in Task 2.

- [ ] **Step 1: Fix the Blazor error-UI contrast (deferred from PR #2's final review)**

The final review of PR #2 found `.btn-danger`'s white text on dark-theme `--danger` failed AA (3.04:1) and fixed it with a `--danger-text` token. The reviewer separately flagged, as an out-of-scope observation not fixed at the time, that `#blazor-error-ui` and `.blazor-error-boundary` have the exact same bug — hardcoded `color: #fff` on the same `var(--danger)` background. Fix both now that the token exists.

In `src/Ruptura.Web/wwwroot/css/app.css`, replace:

```css
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
```

with:

```css
#blazor-error-ui {
    background: var(--danger);
    bottom: 0; left: 0;
    display: none;
    padding: 0.75rem 1.5rem;
    position: fixed;
    width: 100%;
    z-index: 9999;
    color: var(--danger-text);
    font-size: var(--text-sm);
    letter-spacing: 0.05em;
}
#blazor-error-ui .reload { color: var(--danger-text); text-decoration: underline; margin-left: 1rem; }
#blazor-error-ui .dismiss { float: right; cursor: pointer; }
.blazor-error-boundary { background: var(--danger); padding: 1rem; color: var(--danger-text); }
.blazor-error-boundary::after { content: "An error has occurred."; }
```

(Only the three `color: #fff` → `color: var(--danger-text)` substitutions change; everything else is unchanged context, included so the replacement is unambiguous.)

- [ ] **Step 2: Replace `Register.razor`'s hardcoded inline font-size with the type-scale token**

In `src/Ruptura.Web/Pages/Register.razor`, replace:

```razor
                    <ul class="mb-0 mt-1 ps-3" style="font-size:.8rem">
```

with:

```razor
                    <ul class="mb-0 mt-1 ps-3" style="font-size:var(--text-sm)">
```

(`.8rem` = 12.8px; `--text-sm` = 14px (0.875rem) — the nearest token, and above the 11px floor with room to spare. This is the only remaining hardcoded `rem` font-size anywhere in the Auth page group.)

- [ ] **Step 3: Localize the hardcoded "Sistema" eyebrow text on the landing page**

`Index.razor` currently hardcodes the Portuguese word "Sistema" in the pillars section, shown regardless of the selected UI language — the same class of bug already fixed elsewhere in PR #2 (e.g. the GM sidebar section label).

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Landing.Pillars.Title` line and add directly above it:

```xml
  <data name="Landing.Pillars.Eyebrow"><value>System</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Landing.Pillars.Title` line and add directly above it:

```xml
  <data name="Landing.Pillars.Eyebrow"><value>Sistema</value></data>
```

In `src/Ruptura.Web/Pages/Index.razor`, replace:

```razor
            <span class="pillars-eyebrow">Sistema</span>
```

with:

```razor
            <span class="pillars-eyebrow">@L["Landing.Pillars.Eyebrow"]</span>
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/wwwroot/css/app.css src/Ruptura.Web/Pages/Register.razor \
        src/Ruptura.Web/Pages/Index.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "fix: resolve auth-group contrast/localization gaps left from the design-system rework"
```

---

## Task 2: Visual verification pass — Login, Register, Landing

**Files:** none prescribed — this task may touch whatever file a genuine regression it finds lives in (same allowance as the foundation plan's final verification task).

**Interfaces:** none — this task confirms Task 1's deliverable, plus everything these three pages already inherited from PR #2's CSS foundation, together.

- [ ] **Step 1: Build and run the unit test suite**

Run: `dotnet build` (whole solution), then `dotnet test tests/Ruptura.UnitTests`
Expected: 0 build errors; all tests passing (this plan adds no new tests, so the count should match whatever `main` currently has).

- [ ] **Step 2: Launch the app and capture screenshots**

Use the `run` skill to build and serve `Ruptura.Web`. Capture screenshots of:

- `/` (landing) — light and dark, desktop (≥1024px) and mobile (<600px).
- `/login` — light and dark, desktop and mobile.
- `/register` — light and dark, desktop and mobile, **both** the Game Master and Player toggle states (the Player state renders the extra invite-code field via `PlayerFields.razor`).

Confirm for each: no text below 11px, no low-contrast text against its background (including the now-fixed `#blazor-error-ui`/`.blazor-error-boundary` — trigger a real error if practical, e.g. temporarily disconnect the API, to see it rendered; otherwise inspect the computed styles), the landing hero's dark `.landing-hero` section and light `.landing-pillars`/`.landing-quote`/`.landing-cta` sections all read correctly in both themes, and the auth forms are usable and legible on a 390px-wide viewport (labels, inputs, and the submit button don't crowd or overflow).

- [ ] **Step 3: Fix anything the visual pass surfaces**

If a screenshot shows a real regression (not a pre-existing/out-of-scope issue — only things these three pages actually have wrong), fix it in the relevant file directly, keeping the fix small and targeted, then re-verify (rebuild, re-screenshot the affected page). If a finding needs a design decision rather than a bug fix, stop and report `DONE_WITH_CONCERNS`/`BLOCKED` describing it instead of improvising.

- [ ] **Step 4: Commit any fixes made**

If Step 3 produced changes:

```bash
git add -A
git commit -m "fix: address auth-page visual regressions found in verification pass"
```

If Step 3 found nothing to fix, skip this step — a clean pass with no code changes is a valid, complete outcome.

- [ ] **Step 5: Report**

Summarize what was verified (the three pages, both themes, both breakpoints, both Register toggle states) and confirm the Auth page group is now fully aligned with the design system. Note that the next page group in the spec's §8 rollout order is the rest of GM (`GmDashboard`, `GmPlayers`, `GmCampaigns`, `GmCampaignDetail`, `GmFields`, `GmNotifications`, `GmCharacterSheet` — `GmCatalog` is already done from PR #2), which needs its own plan.
