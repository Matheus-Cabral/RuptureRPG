using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Web.Services;

public interface IGuildClientService
{
    Task<ApiResponse<GuildSheetResponse>?> GetGuildAsync(Guid campaignId);
}
