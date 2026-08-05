# Journal & Media Storage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the character journal (`CharacterJournalEntry`: text + image entries, owner-only write) and the local-disk media storage layer (`IFileStorageService`) it and the character portrait depend on — the 11th and final character-sheet module tab, sub-plan #4 of the Character Sheet feature roadmap.

**Architecture:** A new `CharacterJournalEntry` table (real FK to `CharacterSheet`, unlike the rest of the schema's soft-reference convention — this is a fresh table, not a retrofit). Media authorization is path-encoded, not table-backed: `GET /api/media/{*path}` parses the owning entity's type + ID straight out of the storage path and reuses the exact same owner-or-GM checks `CharacterSheetService`/`JournalEntryService` already enforce elsewhere — no new metadata table. `POST /api/media` mutates its target entity server-side (sets `PortraitImagePath`, or appends to a journal entry's `ImagePaths`) instead of requiring a client follow-up write.

**Tech Stack:** Same as the rest of the repo — ASP.NET Core 8 / EF Core 8 / Npgsql, FluentValidation, xUnit + Moq + FluentAssertions + Bogus, Testcontainers.PostgreSql, Blazor WASM (`InputFile` for uploads, base64 data URIs for authenticated image display — see Task 11's note on why).

## Global Constraints

- **Result pattern**: every Application/Infrastructure service method returns `Result` or `Result<T>` from `Ruptura.Application.Common` — never throw business exceptions across layer boundaries.
- **Bilingual localization**: every user-facing string goes through `IStringLocalizer` — API messages via `IStringLocalizer<SharedResources>` (`src/Ruptura.API/Resources/SharedResources.resx` + `.pt-BR.resx`), Blazor UI strings via `IStringLocalizer<AppStrings>` (`src/Ruptura.Web/Resources/AppStrings.resx` + `.pt-BR.resx`). Every task that adds a user-facing string adds both `en` and `pt-BR` entries in the same task.
- **Enum-from-string parsing always pairs `Enum.TryParse` with `Enum.IsDefined`** — `TryParse` alone accepts any int-parseable value regardless of whether it's a real enum member (this exact bug was caught and fixed in the Catalog subsystem; do not reintroduce it for `MediaEntityType`).
- **Unauthorized access returns `NotFound`, not `Forbidden`** — matches the existing convention across `CatalogEntryService`/`CampaignService`/`CharacterSheetService`: a caller with no relationship to a resource never learns whether it exists. This applies to media downloads too — a malformed path, a real-but-unauthorized path, and a nonexistent path all produce the same 404.
- **No FK-avoidance convention for brand-new tables**: `CharacterJournalEntry.CharacterSheetId` gets a real FK with `ON DELETE CASCADE` — the repo's soft-reference convention (`CharacterSheet.OwnerId`, `CatalogEntry.CreatedByGameMasterId`) applies to columns that predate a clean opportunity to add one; this table is created fresh in this plan, so it doesn't inherit that exception.
- **Always reassign a new `List<string>` instance for `ImagePaths`, never mutate in place** (`entry.ImagePaths = [.. entry.ImagePaths, newPath]`, not `entry.ImagePaths.Add(newPath)`) — the EF Core value converter on this property (Task 1) round-trips through a JSON string column; without a `ValueComparer`, EF's change tracker won't notice an in-place list mutation and will silently skip persisting it. Reassigning a new list reference sidesteps the need for a `ValueComparer` entirely — simpler than adding one.
- **`0` means "unlimited"** for both `MediaSettings.MaxFileSizeMb` and `MediaSettings.MaxImagesPerJournalEntry` — never treat `0` as "reject everything."

---

## File Structure

```
src/Ruptura.Domain/Entities/CharacterJournalEntry.cs                (new)
src/Ruptura.Domain/Enums/MediaEntityType.cs                          (new)

src/Ruptura.Infrastructure/Data/Configurations/CharacterJournalEntryConfiguration.cs  (new)
src/Ruptura.Infrastructure/Data/Migrations/...                      (new migration)
src/Ruptura.Infrastructure/Repositories/CharacterJournalEntryRepository.cs  (new)
src/Ruptura.Infrastructure/Services/JournalEntryService.cs           (new)
src/Ruptura.Infrastructure/Services/CharacterSheetService.cs         (modify — AuthorizeAccessAsync, SetPortraitPathAsync)
src/Ruptura.Infrastructure/Services/LocalFileStorageService.cs       (new)
src/Ruptura.Infrastructure/Settings/MediaSettings.cs                 (new)
src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs    (modify — new DI registrations)

src/Ruptura.Application/Interfaces/ICharacterJournalEntryRepository.cs  (new)
src/Ruptura.Application/Interfaces/IJournalEntryService.cs            (new)
src/Ruptura.Application/Interfaces/ICharacterSheetService.cs          (modify — AuthorizeAccessAsync, SetPortraitPathAsync)
src/Ruptura.Application/Interfaces/IFileStorageService.cs             (new)
src/Ruptura.Application/Common/ErrorCodes.cs                          (modify — Journal.*, Media.*)
src/Ruptura.Application/Validators/Journal/CreateJournalEntryRequestValidator.cs  (new)
src/Ruptura.Application/Validators/Journal/UpdateJournalEntryRequestValidator.cs  (new)

src/Ruptura.Shared/Journal/CreateJournalEntryRequest.cs               (new)
src/Ruptura.Shared/Journal/UpdateJournalEntryRequest.cs               (new)
src/Ruptura.Shared/Journal/JournalEntryResponse.cs                    (new)
src/Ruptura.Shared/Media/MediaUploadResponse.cs                       (new)

src/Ruptura.API/Controllers/JournalEntryController.cs                 (new)
src/Ruptura.API/Controllers/MediaController.cs                       (new)
src/Ruptura.API/Resources/SharedResources.resx / .pt-BR.resx          (modify — new keys)
src/Ruptura.API/appsettings.json                                     (modify — MediaSettings section)

src/Ruptura.Web/Services/IJournalEntryClientService.cs               (new)
src/Ruptura.Web/Services/JournalEntryClientService.cs                (new)
src/Ruptura.Web/Services/IMediaClientService.cs                      (new)
src/Ruptura.Web/Services/MediaClientService.cs                       (new)
src/Ruptura.Web/Program.cs                                           (modify — DI registration)
src/Ruptura.Web/Pages/CharacterSheetJournalTab.razor                  (new)
src/Ruptura.Web/Pages/CharacterSheetEditor.razor                     (modify — IsOwner param, Journal tab, portrait upload)
src/Ruptura.Web/Pages/PlayerCharacter.razor                          (modify — pass IsOwner="true")
src/Ruptura.Web/Pages/GmCharacterSheet.razor                         (modify — pass IsOwner="false")
src/Ruptura.Web/Resources/AppStrings.resx / .pt-BR.resx              (modify — new keys)

docker-compose.yml                                                   (modify — character_media volume + MediaSettings env)
docker/api/Dockerfile                                                (modify — create+chown /app/media)
.env.example                                                          (modify — MEDIA_MAX_FILE_SIZE_MB, MEDIA_MAX_IMAGES_PER_ENTRY)

tests/Ruptura.UnitTests/Infrastructure/LocalFileStorageServiceTests.cs  (new)
tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs      (new)
tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs    (modify — AuthorizeAccessAsync/SetPortraitPathAsync tests)
tests/Ruptura.IntegrationTests/Helpers/IntegrationTestFactory.cs     (modify — scratch media root override)
tests/Ruptura.IntegrationTests/Controllers/JournalEntryControllerTests.cs  (new)
tests/Ruptura.IntegrationTests/Controllers/MediaControllerTests.cs    (new)
tests/Ruptura.IntegrationTests/Controllers/JournalMediaFlowTests.cs   (new)
```

---

## Task 1: `CharacterJournalEntry` entity + EF configuration + migration + repository

**Files:**
- Create: `src/Ruptura.Domain/Entities/CharacterJournalEntry.cs`
- Create: `src/Ruptura.Infrastructure/Data/Configurations/CharacterJournalEntryConfiguration.cs`
- Modify: `src/Ruptura.Infrastructure/Data/AppDbContext.cs`
- Create: migration via `dotnet ef migrations add`
- Create: `src/Ruptura.Application/Interfaces/ICharacterJournalEntryRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/CharacterJournalEntryRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Produces: `CharacterJournalEntry` entity with `Id`, `CharacterSheetId`, `Text`, `ImagePaths (List<string>)`, `CreatedAt`, `UpdatedAt`; `ICharacterJournalEntryRepository.GetByCharacterSheetAsync(Guid characterSheetId, CancellationToken ct = default) -> Task<IEnumerable<CharacterJournalEntry>>` (newest-first). Every later task depends on these exact names.

- [ ] **Step 1: Create the `CharacterJournalEntry` entity**

```csharp
namespace Ruptura.Domain.Entities;

public class CharacterJournalEntry
{
    public Guid Id { get; set; }
    public Guid CharacterSheetId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 2: Create `CharacterJournalEntryConfiguration`**

```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CharacterJournalEntryConfiguration : IEntityTypeConfiguration<CharacterJournalEntry>
{
    public void Configure(EntityTypeBuilder<CharacterJournalEntry> builder)
    {
        // ImagePaths round-trips through a jsonb column. The service layer always
        // reassigns a new List<string> instance on change (never mutates in place),
        // so no ValueComparer is needed for the change tracker to notice edits —
        // see this plan's Global Constraints.
        builder.Property(e => e.ImagePaths)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");

        builder.HasIndex(e => e.CharacterSheetId);

        // Fresh table, not a retrofit — gets a real FK unlike the rest of the
        // schema's soft-reference convention (see this plan's Global Constraints).
        builder.HasOne<CharacterSheet>()
            .WithMany()
            .HasForeignKey(e => e.CharacterSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 3: Register the `DbSet` in `AppDbContext`**

Add to `src/Ruptura.Infrastructure/Data/AppDbContext.cs`, alongside the existing `DbSet<CharacterSheet>`:

```csharp
    public DbSet<CharacterJournalEntry> CharacterJournalEntries => Set<CharacterJournalEntry>();
```

- [ ] **Step 4: Build to confirm it compiles**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 5: Generate the migration**

```bash
dotnet ef migrations add AddCharacterJournalEntries \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

- [ ] **Step 6: Verify the migration content directly**

```bash
grep -n "CreateTable\|AddForeignKey\|jsonb" src/Ruptura.Infrastructure/Data/Migrations/*_AddCharacterJournalEntries.cs
```

Expected: a `CreateTable` for `CharacterJournalEntries` with an `ImagePaths` column of type `jsonb`, and an `AddForeignKey` from `CharacterJournalEntries.CharacterSheetId` to `CharacterSheets.Id` with `onDelete: ReferentialAction.Cascade`. If either is missing, the migration is wrong — do not proceed until both are present.

- [ ] **Step 7: Apply the migration and verify against a real database**

```bash
dotnet ef database update \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

Expected: no errors. Confirm the table and FK exist:

```bash
docker compose exec -T db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "\d \"CharacterJournalEntries\""
```

(Adjust the connection method to however this repo's Postgres container is actually reachable if `docker compose exec` doesn't match the local setup — `make migrate`'s underlying command is the reference.)

- [ ] **Step 8: Add `ICharacterJournalEntryRepository`**

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICharacterJournalEntryRepository : IRepository<CharacterJournalEntry>
{
    Task<IEnumerable<CharacterJournalEntry>> GetByCharacterSheetAsync(
        Guid characterSheetId, CancellationToken ct = default);
}
```

- [ ] **Step 9: Implement `CharacterJournalEntryRepository`**

```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CharacterJournalEntryRepository(AppDbContext db)
    : BaseRepository<CharacterJournalEntry>(db), ICharacterJournalEntryRepository
{
    public async Task<IEnumerable<CharacterJournalEntry>> GetByCharacterSheetAsync(
        Guid characterSheetId, CancellationToken ct = default) =>
        await Set
            .Where(e => e.CharacterSheetId == characterSheetId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
}
```

- [ ] **Step 10: Register in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Repositories":

```csharp
        services.AddScoped<ICharacterJournalEntryRepository, CharacterJournalEntryRepository>();
```

- [ ] **Step 11: Build and commit**

Run: `dotnet build` — expect no errors.

```bash
git add src/Ruptura.Domain/Entities/CharacterJournalEntry.cs \
  src/Ruptura.Infrastructure/Data/Configurations/CharacterJournalEntryConfiguration.cs \
  src/Ruptura.Infrastructure/Data/AppDbContext.cs \
  src/Ruptura.Infrastructure/Data/Migrations/ \
  src/Ruptura.Application/Interfaces/ICharacterJournalEntryRepository.cs \
  src/Ruptura.Infrastructure/Repositories/CharacterJournalEntryRepository.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add CharacterJournalEntry entity, migration, and repository"
```

## Task 2: `MediaSettings` + `IFileStorageService`/`LocalFileStorageService` + Docker/env wiring

**Files:**
- Create: `src/Ruptura.Infrastructure/Settings/MediaSettings.cs`
- Create: `src/Ruptura.Application/Interfaces/IFileStorageService.cs`
- Create: `src/Ruptura.Infrastructure/Services/LocalFileStorageService.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Modify: `src/Ruptura.API/appsettings.json`
- Modify: `docker-compose.yml`
- Modify: `docker/api/Dockerfile`
- Modify: `.env.example`
- Test: `tests/Ruptura.UnitTests/Infrastructure/LocalFileStorageServiceTests.cs`

**Interfaces:**
- Produces: `IFileStorageService.SaveAsync(Stream content, string relativePath, CancellationToken ct = default) -> Task`, `.DeleteAsync(string relativePath, CancellationToken ct = default) -> Task`, `.OpenReadAsync(string relativePath, CancellationToken ct = default) -> Task<Stream?>` (null if the file doesn't exist), `.Exists(string relativePath) -> bool`; `MediaSettings { MaxFileSizeMb, MaxImagesPerJournalEntry, RootPath }`. Consumed by `JournalEntryService`/`CharacterSheetService` (Task 3-6) and `MediaController` (Task 9).

- [ ] **Step 1: Create `MediaSettings`**

```csharp
namespace Ruptura.Infrastructure.Settings;

public class MediaSettings
{
    public string RootPath { get; set; } = "/app/media";
    public int MaxFileSizeMb { get; set; } = 5;             // 0 = unlimited
    public int MaxImagesPerJournalEntry { get; set; } = 6;  // 0 = unlimited
}
```

- [ ] **Step 2: Create `IFileStorageService`**

```csharp
namespace Ruptura.Application.Interfaces;

public interface IFileStorageService
{
    Task SaveAsync(Stream content, string relativePath, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default);
    bool Exists(string relativePath);
}
```

`relativePath` is always a forward-slash path like `character-sheets/{sheetId}/portrait-{guid}.jpg` — never an absolute path, and never containing `..` segments (callers pass ids they generated themselves, but the implementation validates anyway as a safety net — see Step 3).

- [ ] **Step 3: Write the failing unit tests**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Options;
using Ruptura.Infrastructure.Services;
using Ruptura.Infrastructure.Settings;

namespace Ruptura.UnitTests.Infrastructure;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ruptura-media-tests-" + Guid.NewGuid());
    private readonly LocalFileStorageService _sut;

    public LocalFileStorageServiceTests()
    {
        _sut = new LocalFileStorageService(Options.Create(new MediaSettings { RootPath = _root }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task SaveAsync_ThenOpenReadAsync_RoundTripsTheSameBytes()
    {
        var bytes = "hello world"u8.ToArray();
        await _sut.SaveAsync(new MemoryStream(bytes), "character-sheets/abc/portrait-1.jpg");

        await using var stream = await _sut.OpenReadAsync("character-sheets/abc/portrait-1.jpg");
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        (await reader.ReadToEndAsync()).Should().Be("hello world");
    }

    [Fact]
    public async Task OpenReadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var result = await _sut.OpenReadAsync("character-sheets/does-not-exist/x.jpg");
        result.Should().BeNull();
    }

    [Fact]
    public void Exists_ReflectsWhetherTheFileIsOnDisk()
    {
        _sut.Exists("character-sheets/abc/nope.jpg").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheFile()
    {
        await _sut.SaveAsync(new MemoryStream("x"u8.ToArray()), "journal-entries/e1/img.png");
        await _sut.DeleteAsync("journal-entries/e1/img.png");

        _sut.Exists("journal-entries/e1/img.png").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenFileDoesNotExist_DoesNotThrow()
    {
        var act = async () => await _sut.DeleteAsync("journal-entries/nope/nope.png");
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("../escape.jpg")]
    [InlineData("character-sheets/../../escape.jpg")]
    [InlineData("character-sheets/abc/../../../escape.jpg")]
    public async Task SaveAsync_RejectsPathTraversal(string maliciousPath)
    {
        var act = async () => await _sut.SaveAsync(new MemoryStream("x"u8.ToArray()), maliciousPath);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

- [ ] **Step 4: Run to confirm it fails (class doesn't exist)**

Run: `dotnet test tests/Ruptura.UnitTests --filter LocalFileStorageServiceTests`
Expected: build error.

- [ ] **Step 5: Implement `LocalFileStorageService`**

```csharp
using Microsoft.Extensions.Options;
using Ruptura.Application.Interfaces;
using Ruptura.Infrastructure.Settings;

namespace Ruptura.Infrastructure.Services;

public class LocalFileStorageService(IOptions<MediaSettings> settings) : IFileStorageService
{
    private readonly string _root = settings.Value.RootPath;

    public async Task SaveAsync(Stream content, string relativePath, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(relativePath);
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public bool Exists(string relativePath) => File.Exists(ResolveSafePath(relativePath));

    // Resolves relativePath under _root and rejects any attempt to escape it —
    // relativePath always comes from ids this codebase generated itself, but this
    // is the last line of defense against a future caller passing raw user input.
    private string ResolveSafePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        var normalizedRoot = Path.GetFullPath(_root) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
            throw new ArgumentException($"Path '{relativePath}' escapes the media root.", nameof(relativePath));

        return fullPath;
    }
}
```

- [ ] **Step 6: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter LocalFileStorageServiceTests`
Expected: PASS (7/7).

- [ ] **Step 7: Register in DI and bind settings**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, add alongside the existing `services.Configure<JwtSettings>(...)`:

```csharp
        services.Configure<MediaSettings>(configuration.GetSection(nameof(MediaSettings)));
```

And under "Core services" (alongside `services.AddSingleton<JwtService>();`):

```csharp
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
```

- [ ] **Step 8: Add the `MediaSettings` section to `appsettings.json`**

In `src/Ruptura.API/appsettings.json`, add alongside the existing `JwtSettings` section:

```json
  "MediaSettings": {
    "RootPath": "/app/media",
    "MaxFileSizeMb": 5,
    "MaxImagesPerJournalEntry": 6
  },
```

- [ ] **Step 9: Add the Docker volume and env wiring**

In `docker-compose.yml`, add a new named volume alongside `postgres_data`/`api_logs`:

```yaml
volumes:
  postgres_data:
  api_logs:
  character_media:
```

Mount it on the `api` service, alongside the existing `api_logs` mount:

```yaml
    volumes:
      - api_logs:/app/logs
      - character_media:/app/media
```

Add the two new env vars to the `api` service's `environment` block, alongside the existing `Jwt__*` entries:

```yaml
      MediaSettings__MaxFileSizeMb: ${MEDIA_MAX_FILE_SIZE_MB}
      MediaSettings__MaxImagesPerJournalEntry: ${MEDIA_MAX_IMAGES_PER_ENTRY}
```

(`MediaSettings__RootPath` is deliberately NOT env-overridable — it's an infra detail tied to the volume mount path, not something a deployer needs to tune; `appsettings.json`'s `/app/media` default already matches the container's mount point.)

- [ ] **Step 10: Add the two new vars to `.env.example`**

```
# ─── Media Storage ────────────────────────────────────────────────────────────
# Max upload size in MB and max images per journal entry. 0 = unlimited.
MEDIA_MAX_FILE_SIZE_MB=5
MEDIA_MAX_IMAGES_PER_ENTRY=6
```

- [ ] **Step 11: Make `/app/media` writable by the non-root container user**

The API container runs as `appuser` (not root) after `USER appuser` in `docker/api/Dockerfile`. A fresh named Docker volume mounted at `/app/media` is owned by root by default, so `appuser` can't write to it unless the directory is created and chowned before the user switch. In `docker/api/Dockerfile`, add this right before the existing `USER appuser` line:

```dockerfile
RUN mkdir -p /app/media && chown -R appuser:appgroup /app/media
```

- [ ] **Step 12: Build and commit**

Run: `dotnet build` — expect no errors.

```bash
git add src/Ruptura.Infrastructure/Settings/MediaSettings.cs \
  src/Ruptura.Application/Interfaces/IFileStorageService.cs \
  src/Ruptura.Infrastructure/Services/LocalFileStorageService.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  src/Ruptura.API/appsettings.json \
  docker-compose.yml docker/api/Dockerfile .env.example \
  tests/Ruptura.UnitTests/Infrastructure/LocalFileStorageServiceTests.cs
git commit -m "feat: add IFileStorageService (local disk) and media Docker/env wiring"
```

## Task 3: `MediaEntityType` enum + `ErrorCodes.Journal`/`ErrorCodes.Media`

Small, standalone task — just the shared vocabulary the next several tasks need.

**Files:**
- Create: `src/Ruptura.Domain/Enums/MediaEntityType.cs`
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Test: `tests/Ruptura.UnitTests/Domain/MediaEntityTypeTests.cs`

**Interfaces:**
- Produces: `MediaEntityType { CharacterSheetPortrait, JournalEntryImage }` (plain `int`-backed enum, same convention as `CatalogEntryType`); `ErrorCodes.Journal.{NotFound, OnlyOwnerCanWrite}`; `ErrorCodes.Media.{InvalidEntityType, FileRequired, FileTooLarge, UnsupportedFileType, TooManyImages, NotFound}`. Consumed by `JournalEntryService` (Task 4-6), `CharacterSheetService` (Task 6), `MediaController` (Task 8).

- [ ] **Step 1: Create the enum**

```csharp
namespace Ruptura.Domain.Enums;

public enum MediaEntityType
{
    CharacterSheetPortrait,
    JournalEntryImage
}
```

- [ ] **Step 2: Add the error codes**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, add two new nested classes alongside the existing ones:

```csharp
    public static class Journal
    {
        public const string NotFound = "Journal.NotFound";
        public const string OnlyOwnerCanWrite = "Journal.OnlyOwnerCanWrite";
    }

    public static class Media
    {
        public const string InvalidEntityType = "Media.InvalidEntityType";
        public const string FileRequired = "Media.FileRequired";
        public const string FileTooLarge = "Media.FileTooLarge";
        public const string UnsupportedFileType = "Media.UnsupportedFileType";
        public const string TooManyImages = "Media.TooManyImages";
        public const string NotFound = "Media.NotFound";
    }
```

`Media.NotFound` is used only for a genuinely malformed `GET /api/media/{*path}` request (unrecognized path shape, bad GUID segment) — an authorized-but-wrong-entity-type or unauthorized-but-real-path request reuses the entity's own `CharacterSheet.NotFound`/`Journal.NotFound` code instead (both already exist and already have resx entries from sub-plan #3 and Task 7 respectively).

- [ ] **Step 3: Write a test proving the `TryParse`+`IsDefined` pairing behaves correctly for this enum**

This mirrors the exact bug class caught in the Catalog subsystem — verify it up front for the new enum rather than assuming the convention will be followed correctly later.

```csharp
using FluentAssertions;
using Ruptura.Domain.Enums;

namespace Ruptura.UnitTests.Domain;

public class MediaEntityTypeTests
{
    [Theory]
    [InlineData("CharacterSheetPortrait", true)]
    [InlineData("JournalEntryImage", true)]
    [InlineData("SomethingElse", false)]
    [InlineData("99", false)]  // TryParse alone would accept this; IsDefined must reject it
    public void TryParseAndIsDefined_TogetherRejectUndefinedValues(string input, bool expectedValid)
    {
        var parsed = Enum.TryParse<MediaEntityType>(input, out var value) && Enum.IsDefined(value);
        parsed.Should().Be(expectedValid);
    }
}
```

- [ ] **Step 4: Run the test**

Run: `dotnet test tests/Ruptura.UnitTests --filter MediaEntityTypeTests`
Expected: PASS (4/4) — this test doesn't exercise any new production code (it's testing a language/BCL behavior pairing), so it should pass immediately; it exists to document and lock in the correct pattern before `MediaController` (Task 9) uses it.

- [ ] **Step 5: Build and commit**

Run: `dotnet build` — expect no errors.

```bash
git add src/Ruptura.Domain/Enums/MediaEntityType.cs \
  src/Ruptura.Application/Common/ErrorCodes.cs \
  tests/Ruptura.UnitTests/Domain/MediaEntityTypeTests.cs
git commit -m "feat: add MediaEntityType enum and Journal/Media error codes"
```

## Task 4: `JournalEntryService` core — `CreateAsync`, `GetByCharacterSheetAsync`, authorization helpers

**Files:**
- Create: `src/Ruptura.Shared/Journal/CreateJournalEntryRequest.cs`
- Create: `src/Ruptura.Shared/Journal/JournalEntryResponse.cs`
- Create: `src/Ruptura.Application/Interfaces/IJournalEntryService.cs`
- Create: `src/Ruptura.Infrastructure/Services/JournalEntryService.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Test: `tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs`

**Interfaces:**
- Consumes: `ICharacterJournalEntryRepository` (Task 1), `ICharacterSheetRepository`/`ICampaignRepository` (existing, from sub-plan #3), `ErrorCodes.Journal` (Task 3).
- Produces: `IJournalEntryService` (full 7-method interface — this task implements 4 of them for real; Task 5 replaces the `UpdateAsync`/`DeleteAsync` stubs, Task 6 replaces the `AppendImagePathAsync` stub). `AuthorizeReadAsync`/`AuthorizeWriteAsync` return the raw `Domain.Entities.CharacterJournalEntry` (not a mapped DTO) — this is a deliberate, narrow exception to "services return Shared DTOs": these two methods exist purely as an internal authorization primitive for `MediaController` (Task 8), which needs the entity's current `ImagePaths`/`CharacterSheetId`, not a public-facing response shape.

- [ ] **Step 1: Create the two Shared DTOs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Journal;

public class CreateJournalEntryRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Text { get; set; } = string.Empty;
}
```

```csharp
namespace Ruptura.Shared.Journal;

public class JournalEntryResponse
{
    public Guid Id { get; set; }
    public Guid CharacterSheetId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Create the full `IJournalEntryService` interface**

```csharp
using Ruptura.Application.Common;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Interfaces;

public interface IJournalEntryService
{
    Task<Result<JournalEntryResponse>> CreateAsync(
        Guid callerId, Guid characterSheetId, CreateJournalEntryRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<JournalEntryResponse>>> GetByCharacterSheetAsync(
        Guid callerId, Guid characterSheetId, CancellationToken ct = default);

    Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId, Guid entryId, UpdateJournalEntryRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default);

    // Internal authorization primitives for MediaController (Task 8) — return the
    // raw entity, not a mapped response. AuthorizeReadAsync allows owner-or-GM;
    // AuthorizeWriteAsync allows owner only (per the design spec's permission
    // matrix: journal images can only be added by the sheet's owner).
    Task<Result<CharacterJournalEntry>> AuthorizeReadAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default);

    Task<Result<CharacterJournalEntry>> AuthorizeWriteAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default);

    Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default);
}
```

`UpdateJournalEntryRequest` doesn't exist yet (Task 5 adds it) — create a minimal, final-shaped stub now so this interface compiles:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Journal;

public class UpdateJournalEntryRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Text { get; set; } = string.Empty;

    public List<string> ImagePaths { get; set; } = [];
}
```

(Save this alongside `CreateJournalEntryRequest.cs`/`JournalEntryResponse.cs` in Step 1's file set — Task 5 will not need to change its shape.)

- [ ] **Step 3: Write the failing unit tests**

```csharp
using Bogus;
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Journal;

namespace Ruptura.UnitTests.Application;

public class JournalEntryServiceTests
{
    private readonly Mock<ICharacterJournalEntryRepository> _journalRepoMock = new();
    private readonly Mock<ICharacterSheetRepository> _sheetRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly JournalEntryService _sut;

    private static readonly Faker Faker = new();

    public JournalEntryServiceTests()
    {
        _sut = new JournalEntryService(
            _journalRepoMock.Object, _sheetRepoMock.Object, _campaignRepoMock.Object, _fileStorageMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AsOwner_PersistsEntryWithEmptyImagePaths()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.AddAsync(It.IsAny<CharacterJournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(ownerId, sheet.Id, new CreateJournalEntryRequest { Text = "Day one." });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Text.Should().Be("Day one.");
        result.Value.ImagePaths.Should().BeEmpty();
        _journalRepoMock.Verify(r => r.AddAsync(
            It.Is<CharacterJournalEntry>(e => e.CharacterSheetId == sheet.Id && e.ImagePaths.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AsNonOwner_ReturnsNotFound()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.CreateAsync(Guid.NewGuid(), sheet.Id, new CreateJournalEntryRequest { Text = "x" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    [Fact]
    public async Task CreateAsync_AsGameMaster_ReturnsNotFound()
    {
        // GM does not get to write the journal — only the owner does (design spec §6).
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.CreateAsync(gmId, sheet.Id, new CreateJournalEntryRequest { Text = "x" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── GetByCharacterSheetAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetByCharacterSheetAsync_AsOwner_ReturnsEntries()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _journalRepoMock.Setup(r => r.GetByCharacterSheetAsync(sheet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" }]);

        var result = await _sut.GetByCharacterSheetAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCharacterSheetAsync_AsCampaignGameMaster_ReturnsEntries()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _journalRepoMock.Setup(r => r.GetByCharacterSheetAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _sut.GetByCharacterSheetAsync(gmId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCharacterSheetAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetByCharacterSheetAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── AuthorizeReadAsync / AuthorizeWriteAsync ─────────────────────────────

    [Fact]
    public async Task AuthorizeReadAsync_AsCampaignGameMaster_Succeeds()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeReadAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeWriteAsync_AsCampaignGameMaster_ReturnsNotFound()
    {
        // GM can read journal images but never write them — see design spec §6.
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.AuthorizeWriteAsync(gmId, entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    [Fact]
    public async Task AuthorizeWriteAsync_AsOwner_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.AuthorizeWriteAsync(ownerId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(entry.Id);
    }
}
```

- [ ] **Step 4: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter JournalEntryServiceTests`
Expected: build error — `JournalEntryService` doesn't exist.

- [ ] **Step 5: Implement `JournalEntryService`**

```csharp
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Journal;

namespace Ruptura.Infrastructure.Services;

public class JournalEntryService(
    ICharacterJournalEntryRepository journalRepo,
    ICharacterSheetRepository sheetRepo,
    ICampaignRepository campaignRepo,
    IFileStorageService fileStorage) : IJournalEntryService
{
    public async Task<Result<JournalEntryResponse>> CreateAsync(
        Guid callerId,
        Guid characterSheetId,
        CreateJournalEntryRequest request,
        CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(characterSheetId, ct);
        if (sheet is null || sheet.OwnerId != callerId)
            return Result.Failure<JournalEntryResponse>(ErrorCodes.Journal.NotFound);

        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(),
            CharacterSheetId = characterSheetId,
            Text = request.Text,
            ImagePaths = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await journalRepo.AddAsync(entry, ct);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result<IEnumerable<JournalEntryResponse>>> GetByCharacterSheetAsync(
        Guid callerId,
        Guid characterSheetId,
        CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(characterSheetId, ct);
        if (sheet is null)
            return Result.Failure<IEnumerable<JournalEntryResponse>>(ErrorCodes.Journal.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<IEnumerable<JournalEntryResponse>>(ErrorCodes.Journal.NotFound);

        var entries = await journalRepo.GetByCharacterSheetAsync(characterSheetId, ct);
        return Result.Success(entries.Select(MapToResponse));
    }

    public async Task<Result<CharacterJournalEntry>> AuthorizeReadAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var sheet = await sheetRepo.GetByIdAsync(entry.CharacterSheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        return Result.Success(entry);
    }

    public async Task<Result<CharacterJournalEntry>> AuthorizeWriteAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var sheet = await sheetRepo.GetByIdAsync(entry.CharacterSheetId, ct);
        if (sheet is null || sheet.OwnerId != callerId)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        return Result.Success(entry);
    }

    public Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId, Guid entryId, UpdateJournalEntryRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 5.");

    public Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 5.");

    public Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 6.");

    // ── Private helpers ───────────────────────────────────────────────────────

    private static JournalEntryResponse MapToResponse(CharacterJournalEntry e) => new()
    {
        Id = e.Id,
        CharacterSheetId = e.CharacterSheetId,
        Text = e.Text,
        ImagePaths = e.ImagePaths,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
```

- [ ] **Step 6: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter JournalEntryServiceTests`
Expected: PASS (9/9).

- [ ] **Step 7: Register in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Application services":

```csharp
        services.AddScoped<IJournalEntryService, JournalEntryService>();
```

- [ ] **Step 8: Build the whole solution**

Run: `dotnet build`
Expected: no errors — the `NotImplementedException` stubs make the class satisfy the interface; nothing calls those three methods yet.

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Shared/Journal/ src/Ruptura.Application/Interfaces/IJournalEntryService.cs \
  src/Ruptura.Infrastructure/Services/JournalEntryService.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs
git commit -m "feat: add JournalEntryService core (CreateAsync, GetByCharacterSheetAsync, authorization helpers)"
```

## Task 5: `JournalEntryService` — `UpdateAsync` (full replace + file cleanup), `DeleteAsync`

**Files:**
- Modify: `src/Ruptura.Infrastructure/Services/JournalEntryService.cs`
- Test: `tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs`

**Interfaces:**
- Consumes: `IFileStorageService.DeleteAsync` (Task 2), `AuthorizeWriteAsync` (Task 4, reused internally — do not duplicate its ownership check).
- Produces: replaces the `UpdateAsync`/`DeleteAsync` stubs from Task 4. Consumed by `JournalEntryController` (Task 7).

**Design note:** per the brainstorm decision recorded in the spec, editing a journal entry is always a **full replace** of `Text` + `ImagePaths` together — there is no text-only edit and no separate "remove one image" endpoint. `UpdateAsync` compares the entry's current `ImagePaths` against `request.ImagePaths`; any path present in the old list but absent from the new one is being removed by this edit, so its file gets deleted from disk. Paths present in `request.ImagePaths` that weren't in the old list are NOT a normal case here (new images always arrive via `POST /api/media`, which calls `AppendImagePathAsync` directly — Task 6) — but `UpdateAsync` doesn't need to special-case that; it simply persists whatever `ImagePaths` list the request provides after deleting the dropped files.

- [ ] **Step 1: Write the failing unit tests**

Add to `tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs`:

```csharp
    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_AsOwner_ReplacesTextAndImagePaths()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "Old", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(ownerId, entry.Id, new UpdateJournalEntryRequest
        {
            Text = "New", ImagePaths = ["a.jpg"]
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Text.Should().Be("New");
        result.Value.ImagePaths.Should().ContainSingle().Which.Should().Be("a.jpg");
    }

    [Fact]
    public async Task UpdateAsync_WhenAnImageIsDropped_DeletesItsFileFromDisk()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.UpdateAsync(ownerId, entry.Id, new UpdateJournalEntryRequest { Text = "x", ImagePaths = ["a.jpg"] });

        _fileStorageMock.Verify(f => f.DeleteAsync("b.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.DeleteAsync("a.jpg", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AsCampaignGameMaster_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateJournalEntryRequest { Text = "y", ImagePaths = [] });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_AsOwner_RemovesEntryAndDeletesAllImageFiles()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(ownerId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(f => f.DeleteAsync("a.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.DeleteAsync("b.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _journalRepoMock.Verify(r => r.Remove(entry), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }
```

- [ ] **Step 2: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter JournalEntryServiceTests`
Expected: the new tests throw `NotImplementedException` and FAIL.

- [ ] **Step 3: Implement `UpdateAsync` and `DeleteAsync`**

Replace the two stub bodies in `src/Ruptura.Infrastructure/Services/JournalEntryService.cs`:

```csharp
    public async Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId,
        Guid entryId,
        UpdateJournalEntryRequest request,
        CancellationToken ct = default)
    {
        var authorized = await AuthorizeWriteAsync(callerId, entryId, ct);
        if (authorized.IsFailure)
            return Result.Failure<JournalEntryResponse>(authorized.Error!);

        var entry = authorized.Value!;
        var droppedPaths = entry.ImagePaths.Except(request.ImagePaths).ToList();
        foreach (var path in droppedPaths)
            await fileStorage.DeleteAsync(path, ct);

        entry.Text = request.Text;
        entry.ImagePaths = request.ImagePaths;
        entry.UpdatedAt = DateTime.UtcNow;

        journalRepo.Update(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var authorized = await AuthorizeWriteAsync(callerId, entryId, ct);
        if (authorized.IsFailure)
            return Result.Failure(authorized.Error!);

        var entry = authorized.Value!;
        foreach (var path in entry.ImagePaths)
            await fileStorage.DeleteAsync(path, ct);

        journalRepo.Remove(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success();
    }
```

- [ ] **Step 4: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter JournalEntryServiceTests`
Expected: PASS (all cases across Tasks 4-5).

- [ ] **Step 5: Run the full unit test suite**

Run: `dotnet test tests/Ruptura.UnitTests`
Expected: all PASS, no regressions.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/JournalEntryService.cs \
  tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs
git commit -m "feat: add JournalEntryService.UpdateAsync/DeleteAsync with file cleanup"
```

## Task 6: Media-support methods — `JournalEntryService.AppendImagePathAsync`, `CharacterSheetService.AuthorizeAccessAsync`/`SetPortraitPathAsync`

**Files:**
- Modify: `src/Ruptura.Infrastructure/Services/JournalEntryService.cs`
- Modify: `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`
- Modify: `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`
- Create: `src/Ruptura.Shared/Media/MediaUploadResponse.cs`
- Test: `tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`

**Interfaces:**
- Produces: `IJournalEntryService.AppendImagePathAsync` now implemented for real. `ICharacterSheetService` gains `AuthorizeAccessAsync(Guid callerId, Guid sheetId, CancellationToken ct = default) -> Result<CharacterSheet>` and `SetPortraitPathAsync(Guid sheetId, string? path, CancellationToken ct = default) -> Result` — both consumed by `MediaController` (Task 8).

**Design note:** `ICharacterSheetService.GetAsync` already contains the exact owner-or-GM check `AuthorizeAccessAsync` needs — this task extracts it so `GetAsync` and `AuthorizeAccessAsync` share one implementation instead of two copies of the same permission logic. `GetAsync`'s existing tests (already in `CharacterSheetServiceTests.cs` from sub-plan #3) must keep passing unmodified after this refactor — they're your regression check that the extraction didn't change `GetAsync`'s observable behavior.

- [ ] **Step 1: Write the failing unit tests**

Add to `tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs`:

```csharp
    // ── AppendImagePathAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AppendImagePathAsync_AddsThePathToTheEntrysImagePaths()
    {
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = Guid.NewGuid(), Text = "x", ImagePaths = ["existing.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AppendImagePathAsync(entry.Id, "new.jpg");

        result.IsSuccess.Should().BeTrue();
        entry.ImagePaths.Should().BeEquivalentTo(["existing.jpg", "new.jpg"]);
        _journalRepoMock.Verify(r => r.Update(entry), Times.Once);
    }

    [Fact]
    public async Task AppendImagePathAsync_WhenEntryDoesNotExist_ReturnsNotFound()
    {
        _journalRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterJournalEntry?)null);

        var result = await _sut.AppendImagePathAsync(Guid.NewGuid(), "x.jpg");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }
```

Add to `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`:

```csharp
    // ── AuthorizeAccessAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AuthorizeAccessAsync_AsOwner_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeAccessAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task AuthorizeAccessAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeAccessAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── SetPortraitPathAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task SetPortraitPathAsync_UpdatesThePortraitPath()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), PortraitImagePath = "old.jpg" };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.SetPortraitPathAsync(sheet.Id, "new.jpg");

        result.IsSuccess.Should().BeTrue();
        sheet.PortraitImagePath.Should().Be("new.jpg");
        _sheetRepoMock.Verify(r => r.Update(sheet), Times.Once);
    }
```

- [ ] **Step 2: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter "JournalEntryServiceTests|CharacterSheetServiceTests"`
Expected: the new `JournalEntryService` tests throw `NotImplementedException`; the new `CharacterSheetService` tests fail to build (methods don't exist).

- [ ] **Step 3: Implement `JournalEntryService.AppendImagePathAsync`**

Replace the stub in `src/Ruptura.Infrastructure/Services/JournalEntryService.cs`:

```csharp
    public async Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure(ErrorCodes.Journal.NotFound);

        entry.ImagePaths = [.. entry.ImagePaths, path];
        entry.UpdatedAt = DateTime.UtcNow;

        journalRepo.Update(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success();
    }
```

(Note this deliberately does NOT re-check ownership — the caller, `MediaController`, already authorized via `AuthorizeWriteAsync` before calling this. Don't add a redundant check here; do make sure `MediaController` in Task 8 never calls this without authorizing first.)

- [ ] **Step 4: Add the two new methods to `ICharacterSheetService`**

In `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`, add:

```csharp
    Task<Result<Domain.Entities.CharacterSheet>> AuthorizeAccessAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default);

    Task<Result> SetPortraitPathAsync(Guid sheetId, string? path, CancellationToken ct = default);
```

(Using the fully-qualified `Domain.Entities.CharacterSheet` here to avoid a naming collision — check the file's existing usings; if `Ruptura.Domain.Entities` isn't already imported, add `using Ruptura.Domain.Entities;` and reference `CharacterSheet` directly instead.)

- [ ] **Step 5: Refactor `CharacterSheetService` — extract `AuthorizeAccessAsync`, add `SetPortraitPathAsync`**

In `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`, replace the existing `GetAsync` method body with one that delegates to the new `AuthorizeAccessAsync`:

```csharp
    public async Task<Result<CharacterSheetResponse>> GetAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default)
    {
        var authorized = await AuthorizeAccessAsync(callerId, sheetId, ct);
        if (authorized.IsFailure)
            return Result.Failure<CharacterSheetResponse>(authorized.Error!);

        return Result.Success(await MapToResponseAsync(authorized.Value!, ct));
    }
```

Then add the two new methods (public, alongside `GetAsync`/`UpdateAsync` — not in the private-helpers region):

```csharp
    public async Task<Result<CharacterSheet>> AuthorizeAccessAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheet>(ErrorCodes.CharacterSheet.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<CharacterSheet>(ErrorCodes.CharacterSheet.NotFound);

        return Result.Success(sheet);
    }

    public async Task<Result> SetPortraitPathAsync(Guid sheetId, string? path, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure(ErrorCodes.CharacterSheet.NotFound);

        sheet.PortraitImagePath = path;
        sheet.UpdatedAt = DateTime.UtcNow;

        sheetRepo.Update(sheet);
        await sheetRepo.SaveChangesAsync(ct);

        return Result.Success();
    }
```

- [ ] **Step 6: Create the `MediaUploadResponse` DTO**

```csharp
namespace Ruptura.Shared.Media;

public class MediaUploadResponse
{
    public string Path { get; set; } = string.Empty;
}
```

- [ ] **Step 7: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter "JournalEntryServiceTests|CharacterSheetServiceTests"`
Expected: PASS, including every pre-existing `CharacterSheetServiceTests` case (especially the original `GetAsync_AsOwner_ReturnsSheet`/`GetAsync_AsCampaignGameMaster_ReturnsSheet`/`GetAsync_AsUnrelatedCaller_ReturnsNotFound`/`GetAsync_WhenSheetDoesNotExist_ReturnsNotFound` tests from sub-plan #3 — these must still pass unmodified, confirming the `AuthorizeAccessAsync` extraction didn't change `GetAsync`'s behavior).

- [ ] **Step 8: Run the full unit test suite**

Run: `dotnet test tests/Ruptura.UnitTests`
Expected: all PASS, no regressions.

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/JournalEntryService.cs \
  src/Ruptura.Application/Interfaces/ICharacterSheetService.cs \
  src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  src/Ruptura.Shared/Media/MediaUploadResponse.cs \
  tests/Ruptura.UnitTests/Application/JournalEntryServiceTests.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs
git commit -m "feat: add media-support authorization methods to CharacterSheetService and JournalEntryService"
```

## Task 7: `JournalEntryController` + validators + localization + integration tests

**Files:**
- Create: `src/Ruptura.Application/Validators/Journal/CreateJournalEntryRequestValidator.cs`
- Create: `src/Ruptura.Application/Validators/Journal/UpdateJournalEntryRequestValidator.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Create: `src/Ruptura.API/Controllers/JournalEntryController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx`
- Modify: `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`
- Test: `tests/Ruptura.IntegrationTests/Controllers/JournalEntryControllerTests.cs`

**Interfaces:**
- Consumes: `IJournalEntryService` (Tasks 4-6).
- Produces: the 4 journal HTTP endpoints. Consumed by `JournalEntryClientService` (Task 9).

Endpoints (exactly as listed in the design spec §6):

```
POST   /api/character-sheets/{characterSheetId:guid}/journal-entries    (owner only)
GET    /api/character-sheets/{characterSheetId:guid}/journal-entries    (owner or campaign's GM)
PUT    /api/character-sheets/{characterSheetId:guid}/journal-entries/{entryId:guid}    (owner only)
DELETE /api/character-sheets/{characterSheetId:guid}/journal-entries/{entryId:guid}    (owner only)
```

- [ ] **Step 1: Write the validators**

```csharp
using FluentValidation;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Validators.Journal;

public class CreateJournalEntryRequestValidator : AbstractValidator<CreateJournalEntryRequest>
{
    public CreateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(10000);
    }
}
```

```csharp
using FluentValidation;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Validators.Journal;

public class UpdateJournalEntryRequestValidator : AbstractValidator<UpdateJournalEntryRequest>
{
    public UpdateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.ImagePaths).NotNull();
    }
}
```

- [ ] **Step 2: Register the validators in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Validators":

```csharp
        services.AddScoped<IValidator<CreateJournalEntryRequest>, CreateJournalEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateJournalEntryRequest>, UpdateJournalEntryRequestValidator>();
```

Add `using Ruptura.Application.Validators.Journal;` and `using Ruptura.Shared.Journal;` to that file's usings.

- [ ] **Step 3: Add the resx keys**

In `src/Ruptura.API/Resources/SharedResources.resx`:

```xml
  <data name="Journal.NotFound"><value>Journal entry not found.</value></data>
  <data name="Journal.OnlyOwnerCanWrite"><value>Only the character's owner can write to the journal.</value></data>
  <data name="Journal.Created"><value>Journal entry created successfully.</value></data>
  <data name="Journal.Updated"><value>Journal entry updated successfully.</value></data>
  <data name="Journal.Deleted"><value>Journal entry deleted successfully.</value></data>
```

In `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`:

```xml
  <data name="Journal.NotFound"><value>Entrada de diário não encontrada.</value></data>
  <data name="Journal.OnlyOwnerCanWrite"><value>Só o dono do personagem pode escrever no diário.</value></data>
  <data name="Journal.Created"><value>Entrada de diário criada com sucesso.</value></data>
  <data name="Journal.Updated"><value>Entrada de diário atualizada com sucesso.</value></data>
  <data name="Journal.Deleted"><value>Entrada de diário apagada com sucesso.</value></data>
```

(`ErrorCodes.Journal.OnlyOwnerCanWrite` is defined in Task 3 but never actually returned by `JournalEntryService` — every write-permission failure there maps to the same `NotFound` as the rest of the app's "don't leak existence" convention. Add the resx key anyway for completeness/future use, but don't expect any test to exercise it going through this controller.)

- [ ] **Step 4: Implement `JournalEntryController`**

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/character-sheets/{characterSheetId:guid}/journal-entries")]
[Authorize]
public class JournalEntryController(
    IJournalEntryService journalEntryService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateJournalEntryRequest> createValidator,
    IValidator<UpdateJournalEntryRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<JournalEntryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid characterSheetId, [FromBody] CreateJournalEntryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await journalEntryService.CreateAsync(callerId, characterSheetId, request, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<JournalEntryResponse>.Ok(result.Value!, localizer["Journal.Created"]));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JournalEntryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCharacterSheet(Guid characterSheetId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await journalEntryService.GetByCharacterSheetAsync(callerId, characterSheetId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<JournalEntryResponse>>.Ok(result.Value!));
    }

    [HttpPut("{entryId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<JournalEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid characterSheetId, Guid entryId, [FromBody] UpdateJournalEntryRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await journalEntryService.UpdateAsync(callerId, entryId, request, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<JournalEntryResponse>.Ok(result.Value!, localizer["Journal.Updated"]));
    }

    [HttpDelete("{entryId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid characterSheetId, Guid entryId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await journalEntryService.DeleteAsync(callerId, entryId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Journal.Deleted"]));
    }
}
```

(`characterSheetId` in the route is unused by `Update`/`Delete` beyond routing — `entryId` alone is enough for `JournalEntryService` to authorize, since an entry already knows its own `CharacterSheetId`. This matches the nesting the spec's endpoint list shows; don't remove the route parameter even though the action body doesn't reference it, since the URL shape is part of the spec.)

- [ ] **Step 5: Write the integration tests**

Read `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs` in full first — reuse its `SetUpCampaignWithMemberAsync`-style helper pattern (register GM, invite+register player, assign to campaign, grant a character) rather than reinventing it.

```csharp
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class JournalEntryControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, Guid SheetId, string PlayerToken, string GmToken)> GrantACharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Journal Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        return (client, sheet.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Create_AsOwner_Returns201()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "First day in the Dungeon." });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = (await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        entry.Text.Should().Be("First day in the Dungeon.");
        entry.ImagePaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_AsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, _, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "GM trying to write." });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCharacterSheet_AsCampaignGameMaster_Returns200()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Entry one." });

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!;
        entries.Should().ContainSingle(e => e.Text == "Entry one.");
    }

    [Fact]
    public async Task GetByCharacterSheet_NewestFirst()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries", new CreateJournalEntryRequest { Text = "Older" });
        await Task.Delay(10); // ensure a distinct CreatedAt
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries", new CreateJournalEntryRequest { Text = "Newer" });

        var response = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entries = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.ToList();

        entries.Should().HaveCount(2);
        entries[0].Text.Should().Be("Newer");
        entries[1].Text.Should().Be("Older");
    }

    [Fact]
    public async Task Update_AsOwner_ReplacesText()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Original" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "Edited", ImagePaths = [] });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        updated.Text.Should().Be("Edited");
    }

    [Fact]
    public async Task Update_AsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Original" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, gmToken);
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "GM trying to edit", ImagePaths = [] });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsOwner_Returns200AndEntryIsGone()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "To be deleted" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entries = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!;
        entries.Should().NotContain(e => e.Id == entry.Id);
    }
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter JournalEntryControllerTests`
Expected: PASS (7/7). Re-run once if a failure looks like the documented Serilog flake before treating it as real.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application/Validators/Journal/ \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  src/Ruptura.API/Controllers/JournalEntryController.cs \
  src/Ruptura.API/Resources/SharedResources.resx src/Ruptura.API/Resources/SharedResources.pt-BR.resx \
  tests/Ruptura.IntegrationTests/Controllers/JournalEntryControllerTests.cs
git commit -m "feat: add JournalEntryController with CRUD endpoints"
```

## Task 8: `MediaController` (upload + download) + test infrastructure + integration tests

**Files:**
- Modify: `tests/Ruptura.IntegrationTests/Helpers/IntegrationTestFactory.cs`
- Create: `src/Ruptura.API/Controllers/MediaController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx`
- Modify: `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`
- Test: `tests/Ruptura.IntegrationTests/Controllers/MediaControllerTests.cs`

**Interfaces:**
- Consumes: `IFileStorageService` (Task 2), `MediaSettings` (Task 2), `MediaEntityType`/`ErrorCodes.Media` (Task 3), `ICharacterSheetService.AuthorizeAccessAsync`/`SetPortraitPathAsync` (Task 6), `IJournalEntryService.AuthorizeReadAsync`/`AuthorizeWriteAsync`/`AppendImagePathAsync` (Tasks 4 & 6).
- Produces: `POST /api/media`, `GET /api/media/{*path}`. Consumed by `MediaClientService` (Task 9).

- [ ] **Step 1: Give the integration test suite a scratch media root**

Real uploaded files must land somewhere other than `appsettings.json`'s `/app/media` (which doesn't exist outside the Docker container) during tests. Modify `tests/Ruptura.IntegrationTests/Helpers/IntegrationTestFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ruptura.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace Ruptura.IntegrationTests.Helpers;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly string _mediaRoot =
        Path.Combine(Path.GetTempPath(), "ruptura-test-media-" + Guid.NewGuid());

    public string MediaRoot => _mediaRoot;

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        if (Directory.Exists(_mediaRoot))
            Directory.Delete(_mediaRoot, recursive: true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MediaSettings:RootPath"] = _mediaRoot
            }));

        builder.ConfigureServices(services =>
        {
            // Replace the real DB with the Testcontainers one
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_db.GetConnectionString()));
        });
    }
}
```

- [ ] **Step 2: Add the resx keys**

In `src/Ruptura.API/Resources/SharedResources.resx`:

```xml
  <data name="Media.InvalidEntityType"><value>Invalid media entity type.</value></data>
  <data name="Media.FileRequired"><value>A file is required.</value></data>
  <data name="Media.FileTooLarge"><value>The file exceeds the maximum allowed size.</value></data>
  <data name="Media.UnsupportedFileType"><value>Unsupported file type. Allowed: JPEG, PNG, WEBP, GIF.</value></data>
  <data name="Media.TooManyImages"><value>This journal entry already has the maximum number of images.</value></data>
  <data name="Media.NotFound"><value>Media not found.</value></data>
```

In `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`:

```xml
  <data name="Media.InvalidEntityType"><value>Tipo de entidade de mídia inválido.</value></data>
  <data name="Media.FileRequired"><value>Um arquivo é obrigatório.</value></data>
  <data name="Media.FileTooLarge"><value>O arquivo excede o tamanho máximo permitido.</value></data>
  <data name="Media.UnsupportedFileType"><value>Tipo de arquivo não suportado. Permitidos: JPEG, PNG, WEBP, GIF.</value></data>
  <data name="Media.TooManyImages"><value>Esta entrada de diário já atingiu o número máximo de imagens.</value></data>
  <data name="Media.NotFound"><value>Mídia não encontrada.</value></data>
```

- [ ] **Step 3: Implement `MediaController`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Settings;
using Ruptura.Shared.Common;
using Ruptura.Shared.Media;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController(
    ICharacterSheetService characterSheetService,
    IJournalEntryService journalEntryService,
    IFileStorageService fileStorage,
    IOptions<MediaSettings> mediaSettings,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MediaUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile? file, [FromForm] string entityType, [FromForm] Guid entityId, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.FileRequired]));

        if (!Enum.TryParse<MediaEntityType>(entityType, out var parsedType) || !Enum.IsDefined(parsedType))
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.InvalidEntityType]));

        var maxBytes = (long)mediaSettings.Value.MaxFileSizeMb * 1024 * 1024;
        if (mediaSettings.Value.MaxFileSizeMb > 0 && file.Length > maxBytes)
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.FileTooLarge]));

        var header = new byte[12];
        await using (var probeStream = file.OpenReadStream())
            await probeStream.ReadAsync(header.AsMemory(0, (int)Math.Min(12, file.Length)), ct);

        var contentType = DetectImageContentType(header);
        if (contentType is null)
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.UnsupportedFileType]));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var extension = ExtensionFor(contentType);

        if (parsedType == MediaEntityType.CharacterSheetPortrait)
        {
            var authorized = await characterSheetService.AuthorizeAccessAsync(callerId, entityId, ct);
            if (authorized.IsFailure)
                return NotFound(ApiResponse.Fail(localizer[authorized.Error!]));

            var sheet = authorized.Value!;
            if (!string.IsNullOrEmpty(sheet.PortraitImagePath))
                await fileStorage.DeleteAsync(sheet.PortraitImagePath, ct);

            var relativePath = $"character-sheets/{entityId}/portrait-{Guid.NewGuid()}{extension}";
            await using (var stream = file.OpenReadStream())
                await fileStorage.SaveAsync(stream, relativePath, ct);

            await characterSheetService.SetPortraitPathAsync(entityId, relativePath, ct);
            return Ok(ApiResponse<MediaUploadResponse>.Ok(new MediaUploadResponse { Path = relativePath }));
        }

        // MediaEntityType.JournalEntryImage
        var authorizedEntry = await journalEntryService.AuthorizeWriteAsync(callerId, entityId, ct);
        if (authorizedEntry.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[authorizedEntry.Error!]));

        var entry = authorizedEntry.Value!;
        if (mediaSettings.Value.MaxImagesPerJournalEntry > 0
            && entry.ImagePaths.Count >= mediaSettings.Value.MaxImagesPerJournalEntry)
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.TooManyImages]));

        var journalRelativePath = $"journal-entries/{entityId}/{Guid.NewGuid()}{extension}";
        await using (var journalStream = file.OpenReadStream())
            await fileStorage.SaveAsync(journalStream, journalRelativePath, ct);

        await journalEntryService.AppendImagePathAsync(entityId, journalRelativePath, ct);
        return Ok(ApiResponse<MediaUploadResponse>.Ok(new MediaUploadResponse { Path = journalRelativePath }));
    }

    [HttpGet("{*path}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string path, CancellationToken ct)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !Guid.TryParse(segments[1], out var entityId))
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        var authorized = segments[0] switch
        {
            "character-sheets" => (await characterSheetService.AuthorizeAccessAsync(callerId, entityId, ct)) as Result,
            "journal-entries" => (await journalEntryService.AuthorizeReadAsync(callerId, entityId, ct)) as Result,
            _ => null
        };

        if (authorized is null)
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));
        if (authorized.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[authorized.Error!]));

        var stream = await fileStorage.OpenReadAsync(path, ct);
        if (stream is null)
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));

        return File(stream, ContentTypeForExtension(Path.GetExtension(path)));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string? DetectImageContentType(byte[] header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return "image/jpeg";
        if (header.Length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return "image/png";
        if (header.Length >= 4 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            return "image/gif";
        if (header.Length >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return "image/webp";
        return null;
    }

    private static string ExtensionFor(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        _ => ""
    };

    private static string ContentTypeForExtension(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}
```

Note the `as Result` cast in `Download`: `AuthorizeAccessAsync` returns `Result<CharacterSheet>` and `AuthorizeReadAsync` returns `Result<CharacterJournalEntry>` — both derive from the non-generic `Result` base class (see `src/Ruptura.Application/Common/Result.cs`), so casting either to the base `Result` type lets the switch expression's two branches share one type and lets `Download` only care about `.IsFailure`/`.Error` here, not the specific payload. Add `using Ruptura.Application.Common;` for `Result`.

- [ ] **Step 4: Write the integration tests**

Read `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs` and `JournalEntryControllerTests.cs` (Task 7) first for the exact helper patterns to reuse. `MultipartFormDataContent` is how `HttpClient` sends a file upload in a test.

```csharp
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Media;

namespace Ruptura.IntegrationTests.Controllers;

public class MediaControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    // A minimal, valid 1x1 PNG (correct magic bytes) used across every upload test.
    private static readonly byte[] TinyPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
        0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
        0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB0, 0x00, 0x00, 0x00,
        0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    private static MultipartFormDataContent BuildUploadForm(string entityType, Guid entityId, byte[]? bytes = null)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes ?? TinyPng);
        content.Add(fileContent, "file", "upload.png");
        content.Add(new StringContent(entityType), "entityType");
        content.Add(new StringContent(entityId.ToString()), "entityId");
        return content;
    }

    private async Task<(HttpClient Client, Guid SheetId, string PlayerToken, string GmToken)> GrantACharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Media Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        return (client, sheet.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Upload_PortraitAsOwner_SavesFileAndUpdatesSheet()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var upload = (await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        upload.Path.Should().StartWith($"character-sheets/{sheetId}/portrait-");
        File.Exists(Path.Combine(factory.MediaRoot, upload.Path)).Should().BeTrue();

        var sheetResponse = await client.GetAsync($"api/character-sheets/{sheetId}");
        var sheet = (await sheetResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheet.PortraitImagePath.Should().Be(upload.Path);
    }

    [Fact]
    public async Task Upload_PortraitReplacement_DeletesTheOldFile()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var firstResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var firstUpload = (await firstResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        var firstFullPath = Path.Combine(factory.MediaRoot, firstUpload.Path);
        File.Exists(firstFullPath).Should().BeTrue();

        await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        File.Exists(firstFullPath).Should().BeFalse();
    }

    [Fact]
    public async Task Upload_PortraitAsUnrelatedPlayer_Returns404()
    {
        var (client, sheetId, _, _) = await GrantACharacterAsync();
        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_JournalImageAsOwner_AppendsToImagePaths()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Photo day" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var response = await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var refreshed = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!
            .Data!.Single(e => e.Id == entry.Id);
        refreshed.ImagePaths.Should().ContainSingle();
    }

    [Fact]
    public async Task Upload_JournalImageAsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "x" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_WithUnrecognizedFileBytes_Returns400()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media",
            BuildUploadForm("CharacterSheetPortrait", sheetId, "not an image"u8.ToArray()));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithInvalidEntityType_Returns400()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("SomethingElse", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Download_PortraitAsOwner_Returns200WithImageBytes()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var uploadResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().BeEquivalentTo(TinyPng);
    }

    [Fact]
    public async Task Download_AsUnrelatedCaller_Returns404()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var uploadResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_WithMalformedPath_Returns404()
    {
        var (client, _, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync("api/media/not-a-real-path");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter MediaControllerTests`
Expected: PASS (10/10). Re-run once if a failure looks like the documented Serilog flake before treating it as real.

- [ ] **Step 6: Commit**

```bash
git add tests/Ruptura.IntegrationTests/Helpers/IntegrationTestFactory.cs \
  src/Ruptura.API/Controllers/MediaController.cs \
  src/Ruptura.API/Resources/SharedResources.resx src/Ruptura.API/Resources/SharedResources.pt-BR.resx \
  tests/Ruptura.IntegrationTests/Controllers/MediaControllerTests.cs
git commit -m "feat: add MediaController with path-authorized upload/download"
```

## Task 9: Web client services — `IJournalEntryClientService`, `IMediaClientService`

**Files:**
- Create: `src/Ruptura.Web/Services/IJournalEntryClientService.cs`
- Create: `src/Ruptura.Web/Services/JournalEntryClientService.cs`
- Create: `src/Ruptura.Web/Services/IMediaClientService.cs`
- Create: `src/Ruptura.Web/Services/MediaClientService.cs`
- Modify: `src/Ruptura.Web/Program.cs`

**Interfaces:**
- Consumes: `JournalEntryResponse`/`CreateJournalEntryRequest`/`UpdateJournalEntryRequest` (Task 4-5), `MediaUploadResponse` (Task 6).
- Produces: `IJournalEntryClientService`, `IMediaClientService.UploadAsync(Stream content, string fileName, string entityType, Guid entityId) -> Task<ApiResponse<MediaUploadResponse>?>`, `IMediaClientService.GetDataUriAsync(string? path) -> Task<string?>`. Consumed by Tasks 10-11.

**Design note on `GetDataUriAsync`:** `<img src="...">` in Blazor WASM has no way to attach an `Authorization: Bearer` header, but every `GET /api/media/{*path}` call requires one (the whole point of Task 8's path-based authorization). The fix used throughout this codebase's Blazor pages — an authenticated `HttpClient` — still works for images: `GetDataUriAsync` fetches the bytes through the normal authenticated `HttpClient` and returns a `data:` URI string (`data:image/png;base64,...`) that `<img src>` can bind to directly, with zero JS interop and no new auth mechanism. Given `MediaSettings.MaxFileSizeMb` already bounds upload size, base64-inflated images staying in memory briefly is an acceptable tradeoff for this app's scale.

- [ ] **Step 1: Create `IJournalEntryClientService`**

```csharp
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;

namespace Ruptura.Web.Services;

public interface IJournalEntryClientService
{
    Task<ApiResponse<IEnumerable<JournalEntryResponse>>?> GetByCharacterSheetAsync(Guid characterSheetId);
    Task<ApiResponse<JournalEntryResponse>?> CreateAsync(Guid characterSheetId, CreateJournalEntryRequest request);
    Task<ApiResponse<JournalEntryResponse>?> UpdateAsync(Guid characterSheetId, Guid entryId, UpdateJournalEntryRequest request);
    Task<ApiResponse?> DeleteAsync(Guid characterSheetId, Guid entryId);
}
```

- [ ] **Step 2: Implement `JournalEntryClientService`**

```csharp
using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;

namespace Ruptura.Web.Services;

public class JournalEntryClientService(IHttpClientFactory factory) : IJournalEntryClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<JournalEntryResponse>>?> GetByCharacterSheetAsync(Guid characterSheetId)
    {
        var response = await Http.GetAsync($"api/character-sheets/{characterSheetId}/journal-entries");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>();
    }

    public async Task<ApiResponse<JournalEntryResponse>?> CreateAsync(Guid characterSheetId, CreateJournalEntryRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/character-sheets/{characterSheetId}/journal-entries", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>();
    }

    public async Task<ApiResponse<JournalEntryResponse>?> UpdateAsync(
        Guid characterSheetId, Guid entryId, UpdateJournalEntryRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/character-sheets/{characterSheetId}/journal-entries/{entryId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid characterSheetId, Guid entryId)
    {
        var response = await Http.DeleteAsync($"api/character-sheets/{characterSheetId}/journal-entries/{entryId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

- [ ] **Step 3: Create `IMediaClientService`**

```csharp
using Ruptura.Shared.Common;
using Ruptura.Shared.Media;

namespace Ruptura.Web.Services;

public interface IMediaClientService
{
    Task<ApiResponse<MediaUploadResponse>?> UploadAsync(Stream content, string fileName, string entityType, Guid entityId);
    Task<string?> GetDataUriAsync(string? path);
}
```

- [ ] **Step 4: Implement `MediaClientService`**

```csharp
using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Media;

namespace Ruptura.Web.Services;

public class MediaClientService(IHttpClientFactory factory) : IMediaClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<MediaUploadResponse>?> UploadAsync(
        Stream content, string fileName, string entityType, Guid entityId)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(entityType), "entityType");
        form.Add(new StringContent(entityId.ToString()), "entityId");

        var response = await Http.PostAsync("api/media", form);
        return await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>();
    }

    public async Task<string?> GetDataUriAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        var response = await Http.GetAsync($"api/media/{path}");
        if (!response.IsSuccessStatusCode) return null;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
    }
}
```

- [ ] **Step 5: Register both in DI**

In `src/Ruptura.Web/Program.cs`, alongside the existing `AddScoped<ICharacterSheetClientService, ...>` line:

```csharp
builder.Services.AddScoped<IJournalEntryClientService, JournalEntryClientService>();
builder.Services.AddScoped<IMediaClientService, MediaClientService>();
```

- [ ] **Step 6: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Web/Services/IJournalEntryClientService.cs src/Ruptura.Web/Services/JournalEntryClientService.cs \
  src/Ruptura.Web/Services/IMediaClientService.cs src/Ruptura.Web/Services/MediaClientService.cs \
  src/Ruptura.Web/Program.cs
git commit -m "feat: add JournalEntryClientService and MediaClientService"
```

## Task 10: `CharacterSheetJournalTab` — the 11th and final character-sheet tab

**Files:**
- Create: `src/Ruptura.Web/Pages/CharacterSheetJournalTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Modify: `src/Ruptura.Web/Pages/PlayerCharacter.razor`
- Modify: `src/Ruptura.Web/Pages/GmCharacterSheet.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `IJournalEntryClientService`, `IMediaClientService` (Task 9).
- Produces: the final tab entry in `CharacterSheetEditor`'s `Tabs` dictionary, bringing the total to all 11 design-spec modules. `CharacterSheetEditor` gains `[Parameter] public bool IsOwner { get; set; }`.

**UI flow (matches the design spec's decisions):** creating an entry is text-only (`POST journal-entries`); once created, the entry is immediately opened in edit mode so the owner can attach images via `POST /api/media` (each upload appends server-side and the tab re-fetches to show the new thumbnail). Editing an existing entry's images means unchecking/removing a thumbnail client-side, then Save — which sends the full remaining `ImagePaths` list via `PUT`, and the server deletes the dropped file. A GM viewing another player's sheet (`IsOwner="false"`) sees the same list with thumbnails but no write controls at all.

- [ ] **Step 1: Add the localization keys**

`AppStrings.resx`:

```xml
  <data name="Sheet.Tab.Journal"><value>Journal</value></data>
  <data name="Journal.Empty"><value>No journal entries yet.</value></data>
  <data name="Journal.NewEntryPlaceholder"><value>What happened today...</value></data>
  <data name="Journal.Add"><value>Add Entry</value></data>
  <data name="Journal.Edit"><value>Edit</value></data>
  <data name="Journal.Save"><value>Save</value></data>
  <data name="Journal.Cancel"><value>Cancel</value></data>
  <data name="Journal.Delete"><value>Delete</value></data>
  <data name="Journal.AddImage"><value>Add Image</value></data>
  <data name="Journal.Uploading"><value>Uploading…</value></data>
```

`AppStrings.pt-BR.resx`:

```xml
  <data name="Sheet.Tab.Journal"><value>Diário</value></data>
  <data name="Journal.Empty"><value>Nenhuma entrada no diário ainda.</value></data>
  <data name="Journal.NewEntryPlaceholder"><value>O que aconteceu hoje...</value></data>
  <data name="Journal.Add"><value>Adicionar Entrada</value></data>
  <data name="Journal.Edit"><value>Editar</value></data>
  <data name="Journal.Save"><value>Salvar</value></data>
  <data name="Journal.Cancel"><value>Cancelar</value></data>
  <data name="Journal.Delete"><value>Apagar</value></data>
  <data name="Journal.AddImage"><value>Adicionar Imagem</value></data>
  <data name="Journal.Uploading"><value>Enviando…</value></data>
```

- [ ] **Step 2: Create `CharacterSheetJournalTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Journal
@inject IStringLocalizer<AppStrings> L
@inject IJournalEntryClientService JournalService
@inject IMediaClientService MediaService

@if (_loading)
{
    <div class="ledger-empty"><span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]</div>
}
else
{
    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }

    @if (IsOwner)
    {
        <div style="display:flex;flex-direction:column;gap:.5rem;max-width:600px;margin-bottom:1.5rem">
            <textarea class="form-control" rows="3" placeholder="@L["Journal.NewEntryPlaceholder"]"
                      @bind="_newText" @bind:event="oninput"></textarea>
            <button class="btn btn-primary btn-sm" style="align-self:flex-start"
                    @onclick="CreateAsync" disabled="@(_creating || string.IsNullOrWhiteSpace(_newText))">
                @if (_creating) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @L["Journal.Add"]
            </button>
        </div>
    }

    @if (_entries.Count == 0)
    {
        <div class="ledger-empty"><p>@L["Journal.Empty"]</p></div>
    }
    else
    {
        <div style="display:flex;flex-direction:column;gap:1.5rem">
            @foreach (var entry in _entries)
            {
                <div style="border-top:1px solid var(--border);padding-top:1rem">
                    <div style="color:var(--text-muted);font-size:.78rem;margin-bottom:.5rem">
                        @entry.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                    </div>

                    @if (_editingId == entry.Id)
                    {
                        <textarea class="form-control" rows="3" @bind="_editText" @bind:event="oninput"></textarea>
                        <div style="display:flex;gap:.5rem;flex-wrap:wrap;margin:.75rem 0">
                            @foreach (var path in _editImagePaths.ToList())
                            {
                                <div style="position:relative">
                                    <img src="@GetThumb(path)" style="width:80px;height:80px;object-fit:cover;border-radius:4px" />
                                    <span class="btn btn-outline-secondary btn-sm" style="position:absolute;top:-8px;right:-8px;padding:0 6px"
                                          @onclick="() => _editImagePaths.Remove(path)">✕</span>
                                </div>
                            }
                        </div>
                        <InputFile OnChange="e => UploadImageAsync(entry.Id, e)" accept="image/*" disabled="@_uploading" />
                        @if (_uploading) { <span class="spinner-border spinner-border-sm ms-2"></span> @L["Journal.Uploading"] }
                        <div style="display:flex;gap:.5rem;margin-top:.75rem">
                            <button class="btn btn-primary btn-sm" @onclick="() => SaveEditAsync(entry.Id)">@L["Journal.Save"]</button>
                            <button class="btn btn-outline-secondary btn-sm" @onclick="CancelEdit">@L["Journal.Cancel"]</button>
                        </div>
                    }
                    else
                    {
                        <p style="white-space:pre-wrap">@entry.Text</p>
                        @if (entry.ImagePaths.Count > 0)
                        {
                            <div style="display:flex;gap:.5rem;flex-wrap:wrap;margin-bottom:.5rem">
                                @foreach (var path in entry.ImagePaths)
                                {
                                    <img src="@GetThumb(path)" style="width:80px;height:80px;object-fit:cover;border-radius:4px" />
                                }
                            </div>
                        }
                        @if (IsOwner)
                        {
                            <div style="display:flex;gap:.5rem">
                                <button class="btn btn-outline-secondary btn-sm" @onclick="() => StartEdit(entry)">@L["Journal.Edit"]</button>
                                <button class="btn btn-outline-secondary btn-sm" @onclick="() => DeleteAsync(entry.Id)">@L["Journal.Delete"]</button>
                            </div>
                        }
                    }
                </div>
            }
        </div>
    }
}

@code {
    [Parameter] public Guid CharacterSheetId { get; set; }
    [Parameter] public bool IsOwner { get; set; }

    private List<JournalEntryResponse> _entries = [];
    private readonly Dictionary<string, string?> _thumbCache = new();
    private bool _loading = true;
    private bool _creating;
    private bool _uploading;
    private string _newText = string.Empty;
    private Guid? _editingId;
    private string _editText = string.Empty;
    private List<string> _editImagePaths = [];
    private string? _errorMessage;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await JournalService.GetByCharacterSheetAsync(CharacterSheetId);
        _entries = result?.Data?.ToList() ?? [];
        foreach (var path in _entries.SelectMany(e => e.ImagePaths))
            await EnsureThumbAsync(path);
        _loading = false;
    }

    private async Task EnsureThumbAsync(string path)
    {
        if (_thumbCache.ContainsKey(path)) return;
        _thumbCache[path] = await MediaService.GetDataUriAsync(path);
    }

    private string? GetThumb(string path) => _thumbCache.GetValueOrDefault(path);

    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(_newText)) return;

        _creating = true;
        _errorMessage = null;
        var result = await JournalService.CreateAsync(CharacterSheetId, new CreateJournalEntryRequest { Text = _newText });

        if (result?.Data is not null)
        {
            _newText = string.Empty;
            await LoadAsync();
            StartEdit(result.Data);
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _creating = false;
    }

    private void StartEdit(JournalEntryResponse entry)
    {
        _editingId = entry.Id;
        _editText = entry.Text;
        _editImagePaths = entry.ImagePaths.ToList();
    }

    private void CancelEdit()
    {
        _editingId = null;
        _editText = string.Empty;
        _editImagePaths = [];
    }

    private async Task SaveEditAsync(Guid entryId)
    {
        _errorMessage = null;
        var result = await JournalService.UpdateAsync(CharacterSheetId, entryId,
            new UpdateJournalEntryRequest { Text = _editText, ImagePaths = _editImagePaths });

        if (result?.Data is not null)
        {
            CancelEdit();
            await LoadAsync();
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }
    }

    private async Task DeleteAsync(Guid entryId)
    {
        _errorMessage = null;
        var result = await JournalService.DeleteAsync(CharacterSheetId, entryId);

        if (result?.Success == true)
            await LoadAsync();
        else
            _errorMessage = result?.Message ?? L["Common.Error"];
    }

    private async Task UploadImageAsync(Guid entryId, InputFileChangeEventArgs e)
    {
        _uploading = true;
        _errorMessage = null;

        await using var stream = e.File.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
        var result = await MediaService.UploadAsync(stream, e.File.Name, "JournalEntryImage", entryId);

        if (result?.Data is not null)
        {
            var refreshed = await JournalService.GetByCharacterSheetAsync(CharacterSheetId);
            _entries = refreshed?.Data?.ToList() ?? _entries;
            var updatedEntry = _entries.FirstOrDefault(x => x.Id == entryId);
            if (updatedEntry is not null)
            {
                _editImagePaths = updatedEntry.ImagePaths.ToList();
                foreach (var path in updatedEntry.ImagePaths)
                    await EnsureThumbAsync(path);
            }
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _uploading = false;
    }
}
```

`e.File.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024)` caps Blazor's own client-side read at 20MB regardless of the server's configured `MaxFileSizeMb` — this is just Blazor's required safety ceiling for `InputFile` (its default is 500KB and it throws if you don't raise it), not a duplicate of the server-side limit; the server still independently enforces `MediaSettings.MaxFileSizeMb` and is the real source of truth.

- [ ] **Step 3: Wire the tab into `CharacterSheetEditor.razor`**

Add `[Parameter] public bool IsOwner { get; set; }` alongside the existing `SheetId`/`CampaignId`/`CanEditStatus` parameters.

In the `Tabs` dictionary, add after `["guildRegistry"] = "Sheet.Tab.GuildRegistry"`:

```csharp
        ["journal"] = "Sheet.Tab.Journal"
```

In the render chain, add after the `guildRegistry` branch:

```razor
        else if (_activeTab == "journal")
        {
            <CharacterSheetJournalTab CharacterSheetId="SheetId" IsOwner="IsOwner" />
        }
```

- [ ] **Step 4: Pass `IsOwner` from both host pages**

In `src/Ruptura.Web/Pages/PlayerCharacter.razor`, change:

```razor
        <CharacterSheetEditor SheetId="_sheetId.Value" CampaignId="CampaignId" CanEditStatus="false" IsOwner="true" />
```

(A player only ever reaches this page for their own character — `SheetService.GetMineAsync` guarantees that — so `IsOwner` is always `true` here, no runtime check needed.)

In `src/Ruptura.Web/Pages/GmCharacterSheet.razor`, change:

```razor
    <CharacterSheetEditor SheetId="SheetId" CampaignId="CampaignId" CanEditStatus="true" IsOwner="false" />
```

(A GM is never the owner of a `CharacterSheet` — `OwnerId` is always a Player — so `IsOwner` is always `false` here too.)

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetJournalTab.razor src/Ruptura.Web/Pages/CharacterSheetEditor.razor \
  src/Ruptura.Web/Pages/PlayerCharacter.razor src/Ruptura.Web/Pages/GmCharacterSheet.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add Journal tab — the 11th and final character sheet module"
```

## Task 11: Real portrait upload in `CharacterSheetEditor`'s header

**Files:**
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`

**Interfaces:**
- Consumes: `IMediaClientService` (Task 9).
- Produces: nothing new for later tasks — this is the plan's last UI change.

- [ ] **Step 1: Inject `IMediaClientService`**

Add alongside the existing `@inject ICharacterSheetClientService SheetService` line:

```razor
@inject IMediaClientService MediaService
```

- [ ] **Step 2: Replace the plain-text portrait input with a real upload control + preview**

Replace:

```razor
        <div>
            <label class="form-label">@L["Sheet.PortraitLabel"]</label>
            <input class="form-control" @bind="_portraitImagePath" @bind:event="oninput" />
        </div>
```

with:

```razor
        <div>
            <label class="form-label">@L["Sheet.PortraitLabel"]</label>
            <div style="display:flex;align-items:center;gap:.5rem">
                @if (_portraitDataUri is not null)
                {
                    <img src="@_portraitDataUri" style="width:48px;height:48px;object-fit:cover;border-radius:4px" />
                }
                <InputFile OnChange="UploadPortraitAsync" accept="image/*" disabled="@_uploadingPortrait" />
                @if (_uploadingPortrait) { <span class="spinner-border spinner-border-sm"></span> }
            </div>
        </div>
```

- [ ] **Step 3: Add the two new fields and resolve the preview on load**

Add alongside the existing `private string? _portraitImagePath;` field:

```csharp
    private string? _portraitDataUri;
    private bool _uploadingPortrait;
```

In `LoadAsync`, right after `_portraitImagePath = result.Data.PortraitImagePath;`, add:

```csharp
            _portraitDataUri = await MediaService.GetDataUriAsync(_portraitImagePath);
```

- [ ] **Step 4: Add the upload handler**

```csharp
    private async Task UploadPortraitAsync(InputFileChangeEventArgs e)
    {
        _uploadingPortrait = true;
        _errorMessage = null;

        await using var stream = e.File.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
        var result = await MediaService.UploadAsync(stream, e.File.Name, "CharacterSheetPortrait", SheetId);

        if (result?.Data is not null)
        {
            _portraitImagePath = result.Data.Path;
            _portraitDataUri = await MediaService.GetDataUriAsync(_portraitImagePath);
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _uploadingPortrait = false;
    }
```

(The upload already sets `CharacterSheet.PortraitImagePath` server-side — Task 6/8's design. The existing `SaveAsync`'s `UpdateCharacterSheetRequest.PortraitImagePath = _portraitImagePath` re-sends the same value on the next Save, which is a harmless no-op re-confirmation, not a conflicting write.)

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetEditor.razor
git commit -m "feat: wire real portrait upload with preview in CharacterSheetEditor"
```

## Task 12: End-to-end flow test — journal lifecycle + media lifecycle + portrait replace

**Files:**
- Create: `tests/Ruptura.IntegrationTests/Controllers/JournalMediaFlowTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 1-8 (unchanged). This is the plan's final task — pure test coverage tying the whole feature together end-to-end, the way `CharacterSheetFlowTests`/`CatalogFlowTests` did for their sub-plans.

- [ ] **Step 1: Write the flow test**

```csharp
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Media;

namespace Ruptura.IntegrationTests.Controllers;

public class JournalMediaFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private static readonly byte[] TinyPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
        0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
        0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB0, 0x00, 0x00, 0x00,
        0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    private static MultipartFormDataContent BuildUploadForm(string entityType, Guid entityId) =>
        new()
        {
            { new ByteArrayContent(TinyPng), "file", "photo.png" },
            { new StringContent(entityType), "entityType" },
            { new StringContent(entityId.ToString()), "entityId" }
        };

    [Fact]
    public async Task FullFlow_JournalLifecycleMediaLifecyclePortraitReplace_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Journal/Media E2E" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // 1. Player creates a journal entry (text-only at creation).
        AuthHelper.SetBearerToken(client, player.AccessToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries",
            new CreateJournalEntryRequest { Text = "Arrived at the Dungeon gates." });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        entry.ImagePaths.Should().BeEmpty();

        // 2. Player attaches two images.
        var upload1 = (await (await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        var upload2 = (await (await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        File.Exists(Path.Combine(factory.MediaRoot, upload1.Path)).Should().BeTrue();
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeTrue();

        var afterUploads = (await (await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries"))
            .Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.Single(e => e.Id == entry.Id);
        afterUploads.ImagePaths.Should().BeEquivalentTo([upload1.Path, upload2.Path]);

        // 3. Player edits the entry, dropping the first image — its file must be deleted.
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "Arrived at the Dungeon gates. (edited)", ImagePaths = [upload2.Path] });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        File.Exists(Path.Combine(factory.MediaRoot, upload1.Path)).Should().BeFalse();
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeTrue();

        // 4. Player deletes the entry — its remaining image file must be deleted too.
        var deleteResponse = await client.DeleteAsync($"api/character-sheets/{sheet.Id}/journal-entries/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeFalse();

        // 5. Player uploads a portrait, then replaces it — the old file must be deleted.
        var portrait1 = (await (await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheet.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        File.Exists(Path.Combine(factory.MediaRoot, portrait1.Path)).Should().BeTrue();

        var portrait2 = (await (await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheet.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        File.Exists(Path.Combine(factory.MediaRoot, portrait1.Path)).Should().BeFalse();
        File.Exists(Path.Combine(factory.MediaRoot, portrait2.Path)).Should().BeTrue();

        var refreshedSheet = (await (await client.GetAsync($"api/character-sheets/{sheet.Id}"))
            .Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        refreshedSheet.PortraitImagePath.Should().Be(portrait2.Path);

        // 6. The portrait is downloadable by its owner.
        var downloadResponse = await client.GetAsync($"api/media/{portrait2.Path}");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await downloadResponse.Content.ReadAsByteArrayAsync()).Should().BeEquivalentTo(TinyPng);

        // 7. The GM can read the journal (now empty) and the portrait, but cannot write either.
        AuthHelper.SetBearerToken(client, gm.AccessToken);
        var gmJournalRead = await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries");
        gmJournalRead.StatusCode.Should().Be(HttpStatusCode.OK);
        (await gmJournalRead.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.Should().BeEmpty();

        var gmPortraitDownload = await client.GetAsync($"api/media/{portrait2.Path}");
        gmPortraitDownload.StatusCode.Should().Be(HttpStatusCode.OK);

        var gmJournalWrite = await client.PostAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries",
            new CreateJournalEntryRequest { Text = "GM trying to write." });
        gmJournalWrite.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 8. A completely unrelated GM (different campaign) is blocked from everything.
        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        (await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync($"api/media/{portrait2.Path}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 2: Run the flow test**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullFlow_JournalLifecycleMediaLifecyclePortraitReplace_Succeeds`
Expected: PASS. Re-run once if it looks like the documented Serilog flake.

- [ ] **Step 3: Run the entire test suite one final time**

```bash
dotnet build
dotnet test tests/Ruptura.UnitTests
dotnet test tests/Ruptura.IntegrationTests
```

Expected: `dotnet build` clean; unit tests all PASS; integration tests all PASS (re-run once if 1-2 unrelated failures match the documented pre-existing Serilog flake — if the same test fails twice in a row, treat it as real and report it).

- [ ] **Step 4: Commit**

```bash
git add tests/Ruptura.IntegrationTests/Controllers/JournalMediaFlowTests.cs
git commit -m "test: add end-to-end journal/media/portrait flow test"
```
