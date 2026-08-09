# Image UX (UI-A) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** Loaded-image UX — hide the upload control on loaded single-slot images, show an edit (pencil) icon on hover, and let any image expand to full screen (lightbox) on click.
**Scope note:** First sub-project of the "UI adjustments + GM tools" roadmap. UI-B (structured detail panels) and the GM-tools subsystems are separate specs.

---

## 1. Goal & Scope

Improve how images are viewed and edited across the app:
- **Single-slot images** (character portrait, guild emblem): a loaded image no longer shows a separate upload button. Hovering reveals a pencil (edit) icon; clicking the pencil opens the file picker to replace the image; clicking the image body opens it full-screen. An empty slot shows a styled clickable placeholder that opens the file picker.
- **Any image expands on click** via a global lightbox (character portrait, guild emblem, and every journal-entry thumbnail).
- **Journal gallery** (multi-image): each thumbnail gains click-to-expand; the existing "add image" button and the edit-mode ✕ remove stay unchanged.

**Out of scope:** resizing/cropping images, zoom/pan inside the lightbox, prev/next carousel navigation, and any change to the upload/storage pipeline (`IMediaClientService`, `IFileStorageService`, `MediaLimits`).

## 2. Key Decisions (settled in brainstorming, 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Hover-pencil + hidden-upload pattern | Single-slot images only (portrait, emblem) |
| 2 | Journal gallery | Click-to-expand only; add/remove unchanged |
| 3 | Loaded single-slot preview size | ~112×112 (up from 48×48) |
| 4 | Empty single-slot | Styled clickable placeholder/dropzone → file picker |
| 5 | Lightbox | Global, service-driven, fit-to-screen; no zoom/pan/carousel |

## 3. Architecture

Two reusable pieces, following the existing app patterns:

**3.1 Global lightbox** — mirrors the existing `ConfirmService` (DI-scoped, `Services/`) + `ConfirmDialog` (`Layout/`, mounted once in `MainLayout`) pattern:
- `LightboxService` (`Services/`): holds the current image; `Show(string dataUri, string? alt = null)` and `Close()`; an `OnChange` event; a `Current` value (`null` when closed).
- `LightboxContainer` (`Layout/`): mounted once in `MainLayout`. When `Current` is set, renders a dark overlay + the image (`object-fit: contain`, `max-width: 90vw`, `max-height: 90vh`) + a close (✕) button. Closes on overlay click, on the ✕, and on **Esc**. The image element stops click propagation so clicking the image itself doesn't close it. Mirrors `.confirm-overlay`/`.confirm-box` structure and focus handling.

**3.2 `ImageSlot` component** (`Shared/`) — DRY for the two near-identical single-image upload slots (portrait in `CharacterSheetEditor`, emblem in `GuildSheet`):
- Parameters: `DataUri` (`string?`), `Size` (`int`, default `112`), `Uploading` (`bool`), `Disabled` (`bool`), `Alt` (`string`), `OnFilePicked` (`EventCallback<InputFileChangeEventArgs>`).
- Injects `LightboxService`.
- Renders one of three states (see §4).

**3.3 Journal thumbnails** (`CharacterSheetJournalTab`): each `<img>` gains an `@onclick` that calls `Lightbox.Show(dataUri, alt)`; markup otherwise unchanged (add button, ✕ remove, `_uploading` spinner all stay).

## 4. Component Behavior — `ImageSlot`

- **Loaded** (`DataUri` present, not uploading): a `Size`×`Size` preview (`object-fit: cover`). On hover, a pencil edit button overlays (e.g. top-right). Clicking the pencil triggers a hidden `<InputFile>` (→ `OnFilePicked`), with `@onclick:stopPropagation` so it does not also open the lightbox. Clicking the image body calls `Lightbox.Show(DataUri, Alt)`.
- **Empty** (`DataUri` null/empty, not uploading): a styled placeholder/dropzone at `Size`×`Size` (icon + localized label, e.g. "Upload image") wrapping an `<InputFile>` (→ `OnFilePicked`). No lightbox.
- **Uploading** (`Uploading` true): a spinner over the slot; interactions disabled.
- `Disabled` (e.g. a player viewing read-only, or a guild version in flight): no pencil / no file picker; the image still opens in the lightbox on click.

Consuming pages keep their existing state and handlers: `CharacterSheetEditor` passes `_portraitDataUri` + `UploadPortraitAsync` + `_uploadingPortrait`; `GuildSheet` passes `_emblemDataUri` + `UploadEmblemAsync` + `_uploadingEmblem`. The upload methods, `MediaService` calls, and version handling are unchanged — only the markup they render is replaced by `<ImageSlot ... />`.

## 5. Lightbox Image Source

The lightbox shows the same data-URI source the preview/thumbnail already uses (`MediaService.GetDataUriAsync` result for portrait/emblem; the journal thumb cache entry). If a journal thumbnail is a downscaled variant, the plan may fetch the full image for the lightbox; portrait/emblem already load the stored image, displayed small only via CSS, so full resolution is available for the expand with no extra fetch.

## 6. i18n / Styling / Testing

- **i18n:** every new visible/aria string via `IStringLocalizer<AppStrings>` in BOTH `AppStrings.resx` (en) and `AppStrings.pt-BR.resx` — pencil aria-label ("Change image"/"Trocar imagem"), empty-slot label ("Upload image"/"Enviar imagem"), lightbox close aria-label ("Close"/"Fechar"). Keys unaccented ASCII; identical key sets across cultures.
- **Styling:** design-system tokens only; new classes `.image-slot`, `.image-slot-preview`, `.image-slot-edit`, `.image-slot-empty`, `.lightbox-overlay`, `.lightbox-img`, `.lightbox-close`, mirroring the `.confirm-*` conventions (radius token, colors, theme-aware).
- **Testing:** no bUnit harness in the project — verify via clean `dotnet build` and manual/visual check (open a portrait/emblem: hover shows pencil, pencil replaces, image click expands, Esc/overlay/✕ close; journal thumb expands; empty slot shows placeholder → picker). Note the manual verification in the task report.

## 7. Component/Data Impact

| Change | Kind |
|--------|------|
| `LightboxService` (`Services/`) + `LightboxContainer` (`Layout/`, mounted in `MainLayout`) + DI registration | New |
| `ImageSlot` (`Shared/`) | New |
| `CharacterSheetEditor` portrait block → `<ImageSlot>` | Modify |
| `GuildSheet` emblem block → `<ImageSlot>` | Modify |
| `CharacterSheetJournalTab` thumbnails → click-to-expand | Modify |
| `app.css` (`.image-slot*`, `.lightbox*`) + `AppStrings` resx pair | Modify |

No backend, DTO, or storage changes.
