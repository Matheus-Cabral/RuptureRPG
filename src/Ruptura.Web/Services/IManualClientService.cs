namespace Ruptura.Web.Services;

public interface IManualClientService
{
    Task<string?> GetManualAsync(ManualType type, CancellationToken ct = default);
}
