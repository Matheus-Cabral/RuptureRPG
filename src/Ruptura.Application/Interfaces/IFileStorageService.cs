namespace Ruptura.Application.Interfaces;

public interface IFileStorageService
{
    Task SaveAsync(Stream content, string relativePath, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken ct = default);
    bool Exists(string relativePath);
}
