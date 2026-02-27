using Microsoft.OpenApi.Models;

public interface IOpenApiLoader
{
    OpenApiDocument? LoadFromPath(string path);
    Task<OpenApiDocument> LoadFromUrlAsync(string url);

    //Task<OpenApiDocument> LoadFromServerAsync(IEnumerable<string> url, int latestVersion);
    Task<OpenApiDocument?>  LoadFromValidServerAsync(string serverUrl, int latestVersion);
    Task<List<string>> ValidateServersAsync(IEnumerable<string> urls);
}