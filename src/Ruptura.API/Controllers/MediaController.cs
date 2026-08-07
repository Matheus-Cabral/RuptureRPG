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
    IGuildSheetService guildService,
    IFileStorageService fileStorage,
    IOptions<MediaSettings> mediaSettings,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MediaUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile? file, [FromForm] string entityType, [FromForm] Guid entityId,
        [FromForm] uint version, CancellationToken ct)
    {
        // `version` is the caller's expected guild xmin — only the GuildEmblem branch reads it;
        // the portrait/journal branches ignore it (absent form field defaults to 0).
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.FileRequired]));

        if (!Enum.TryParse<MediaEntityType>(entityType, out var parsedType) || !Enum.IsDefined(parsedType))
            return BadRequest(ApiResponse.Fail(localizer[ErrorCodes.Media.InvalidEntityType]));

        if (ExceedsSizeLimit(file.Length, mediaSettings.Value.MaxFileSizeMb))
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

            var setPortraitResult = await characterSheetService.SetPortraitPathAsync(entityId, relativePath, ct);
            if (setPortraitResult.IsFailure)
            {
                // The file was already saved to disk but the entity mutation failed —
                // delete it rather than leave an orphaned file the client believes is linked.
                await fileStorage.DeleteAsync(relativePath, ct);
                return BadRequest(ApiResponse.Fail(localizer[setPortraitResult.Error!]));
            }

            return Ok(ApiResponse<MediaUploadResponse>.Ok(new MediaUploadResponse { Path = relativePath }));
        }

        if (parsedType == MediaEntityType.GuildEmblem)
        {
            var authorized = await guildService.AuthorizeGuildAccessByIdAsync(callerId, entityId, ct);
            if (authorized.IsFailure)
                return NotFound(ApiResponse.Fail(localizer[authorized.Error!]));

            var existingResult = await guildService.GetEmblemPathAsync(entityId, ct);
            if (existingResult.IsFailure)
                return NotFound(ApiResponse.Fail(localizer[existingResult.Error!]));
            if (!string.IsNullOrEmpty(existingResult.Value))
                await fileStorage.DeleteAsync(existingResult.Value, ct);

            var relativePath = $"guild-sheets/{entityId}/emblem-{Guid.NewGuid()}{extension}";
            await using (var stream = file.OpenReadStream())
                await fileStorage.SaveAsync(stream, relativePath, ct);

            var setResult = await guildService.SetEmblemPathAsync(entityId, relativePath, version, ct);
            if (setResult.IsFailure)
            {
                // The file was already saved to disk but the entity mutation failed — delete it
                // rather than leave an orphaned file the client believes is linked. Fail closed on
                // BOTH branches: a stale version → 409 (client refetches), anything else → 400.
                await fileStorage.DeleteAsync(relativePath, ct);
                return setResult.Error == ErrorCodes.Guild.Conflict
                    ? Conflict(ApiResponse.Fail(localizer[setResult.Error!]))
                    : BadRequest(ApiResponse.Fail(localizer[setResult.Error!]));
            }

            return Ok(ApiResponse<MediaUploadResponse>.Ok(
                new MediaUploadResponse { Path = relativePath, Version = setResult.Value }));
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

        var appendResult = await journalEntryService.AppendImagePathAsync(entityId, journalRelativePath, ct);
        if (appendResult.IsFailure)
        {
            await fileStorage.DeleteAsync(journalRelativePath, ct);
            return BadRequest(ApiResponse.Fail(localizer[appendResult.Error!]));
        }

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
            "guild-sheets" => (await guildService.AuthorizeGuildAccessByIdAsync(callerId, entityId, ct)) as Result,
            _ => null
        };

        if (authorized is null || authorized.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));

        Stream? stream;
        try
        {
            stream = await fileStorage.OpenReadAsync(path, ct);
        }
        catch (ArgumentException)
        {
            // Path escapes the configured media root (e.g. crafted "../" segments
            // after a valid-looking prefix). Treat exactly like any other
            // not-found case — never let this distinguish itself as a 500.
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));
        }

        if (stream is null)
            return NotFound(ApiResponse.Fail(localizer[ErrorCodes.Media.NotFound]));

        return File(stream, ContentTypeForExtension(Path.GetExtension(path)));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    // Extracted so it can be unit-tested directly (0 = unlimited must never reject;
    // a real configured limit must reject anything strictly larger). Internal +
    // InternalsVisibleTo(Ruptura.UnitTests) rather than public, since this is an
    // implementation detail of the upload size check, not a public API surface.
    internal static bool ExceedsSizeLimit(long fileLength, int maxFileSizeMb) =>
        maxFileSizeMb > 0 && fileLength > (long)maxFileSizeMb * 1024 * 1024;

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
