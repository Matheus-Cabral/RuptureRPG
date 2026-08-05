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
