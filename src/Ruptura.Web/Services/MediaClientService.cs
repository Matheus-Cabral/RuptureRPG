using System.Net;
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

        // Deliberately NOT gated on response.IsSuccessStatusCode: MediaController's own
        // business-logic rejections (400 with ApiResponse.Fail(...), e.g. TooManyImages,
        // FileTooLarge) are legitimate JSON bodies whose specific localized Message callers
        // rely on — only guard against a body that isn't JSON at all (e.g. a bare 413
        // straight from Kestrel, bypassing the controller entirely), which must not
        // propagate as an unhandled exception.
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>();
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<MediaUploadResult> UploadEmblemAsync(
        Stream content, string fileName, Guid guildId, uint version)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent("GuildEmblem"), "entityType");
        form.Add(new StringContent(guildId.ToString()), "entityId");
        form.Add(new StringContent(version.ToString()), "version");

        var response = await Http.PostAsync("api/media", form);

        // Same JSON-body guard as UploadAsync, but also surface the 409 distinctly so the page
        // can conflict-toast + reload instead of showing a generic error.
        ApiResponse<MediaUploadResponse>? body;
        try
        {
            body = await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>();
        }
        catch (System.Text.Json.JsonException)
        {
            body = null;
        }

        return new MediaUploadResult(body, response.StatusCode == HttpStatusCode.Conflict);
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
