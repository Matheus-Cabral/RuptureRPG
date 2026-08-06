using Ruptura.Shared.Common;
using Ruptura.Shared.Notifications;

namespace Ruptura.Web.Services;

public interface INotificationClientService
{
    Task<ApiResponse<IEnumerable<NotificationGroupResponse>>?> GetMineAsync();
    Task<ApiResponse?> PromoteAsync(Guid id);
    Task<ApiResponse?> DismissAsync(Guid id);
}
