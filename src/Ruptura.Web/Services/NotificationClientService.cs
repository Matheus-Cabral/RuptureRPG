using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Notifications;

namespace Ruptura.Web.Services;

public class NotificationClientService(IHttpClientFactory factory) : INotificationClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<NotificationGroupResponse>>?> GetMineAsync()
    {
        var response = await Http.GetAsync("api/notifications");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<NotificationGroupResponse>>>();
    }

    public async Task<ApiResponse?> PromoteAsync(Guid id)
    {
        var response = await Http.PostAsync($"api/notifications/{id}/promote", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse?> DismissAsync(Guid id)
    {
        var response = await Http.PostAsync($"api/notifications/{id}/dismiss", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
