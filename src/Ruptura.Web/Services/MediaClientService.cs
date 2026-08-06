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
