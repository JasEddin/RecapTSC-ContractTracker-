using Microsoft.OpenApi.Models;

public interface IOpenApiLoader
{
    OpenApiDocument LoadFromPath(string path);
    Task<OpenApiDocument> LoadFromUrlAsync(string url);

    Task<OpenApiDocument> LoadFromServerAsync(IEnumerable<string> url, int latestVersion);

    Task< Uri? > ValidateServersAsync(IEnumerable<string> urls, int latestVersion);
}